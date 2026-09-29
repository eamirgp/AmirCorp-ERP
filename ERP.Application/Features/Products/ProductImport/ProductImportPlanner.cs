using System.Globalization;
using ERP.Application.Common.Formatting;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Products;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.Products.ProductImport
{
    /// <summary>
    /// Decide qué pasará con cada fila de la planilla: crear, actualizar, omitir, sin cambios o error.
    /// Valida con las mismas reglas de <see cref="Product"/>. No guarda nada: la vista previa y la
    /// confirmación usan este mismo plan, así nunca pueden decidir distinto.
    /// </summary>
    internal sealed class ProductImportPlanner
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;

        public ProductImportPlanner(IProductRepository productRepository, IUnitOfMeasureRepository unitOfMeasureRepository)
        {
            _productRepository = productRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
        }

        public async Task<IReadOnlyList<ProductImportEntry>> BuildAsync(IReadOnlyList<ProductSheetRow> rows, bool updateExisting)
        {
            var units = await _unitOfMeasureRepository.ListAllAsync();
            var unitNames = units.ToDictionary(u => u.Code, u => u.Name);

            var codes = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.Code))
                .Select(r => Product.NormalizeCode(r.Code!.Trim()))
                .Distinct()
                .ToArray();

            var existing = (await _productRepository.GetByCodesAsync(codes)).ToDictionary(p => p.Code);
            var firstRowByCode = new Dictionary<string, int>();
            var entries = new List<ProductImportEntry>(rows.Count);

            foreach (var row in rows)
            {
                var errors = new List<string>();
                var unit = ParseUnit(row.UnitOfMeasure, units, errors);
                var igv = ParseIgvAffectation(row.IgvAffectation, errors);
                var price = ParsePrice(row, errors);

                // El dominio valida código, nombre y precio. Si falta la unidad o la afectación, se usa un valor
                // cualquiera solo para poder revisar el resto de la fila; la fila igual queda con error.
                Product? candidate = null;
                try
                {
                    candidate = Product.Create(row.Code?.Trim() ?? "", row.Name?.Trim() ?? "", unit?.Code ?? PlaceholderUnitCode, igv ?? IgvAffectation.Gravado, price ?? 0);
                }
                catch (DomainException ex)
                {
                    errors.Add(ex.Message);
                }

                var code = candidate?.Code ?? row.Code?.Trim();

                if (candidate is not null)
                {
                    if (firstRowByCode.TryGetValue(candidate.Code, out var firstRow))
                        errors.Add($"El código interno {candidate.Code} está repetido: ya aparece en la fila {firstRow}.");
                    else
                        firstRowByCode[candidate.Code] = row.RowNumber;
                }

                if (errors.Count > 0 || candidate is null)
                {
                    entries.Add(new ProductImportEntry(row.RowNumber, code, row.Name?.Trim(), ProductImportAction.Error, errors, [], null, null));
                    continue;
                }

                if (!existing.TryGetValue(candidate.Code, out var current))
                {
                    // Se muestran los valores que se crearán, para detectar antes de guardar un precio o unidad mal leídos.
                    entries.Add(new ProductImportEntry(row.RowNumber, code, candidate.Name, ProductImportAction.Create, [], Values(candidate, unitNames), candidate, null));
                    continue;
                }

                if (!updateExisting)
                {
                    entries.Add(new ProductImportEntry(row.RowNumber, code, candidate.Name, ProductImportAction.Skip, [], [], null, null));
                    continue;
                }

                var changes = Diff(current, candidate, unitNames);
                entries.Add(new ProductImportEntry(
                    row.RowNumber,
                    code,
                    candidate.Name,
                    changes.Count > 0 ? ProductImportAction.Update : ProductImportAction.Unchanged,
                    [],
                    changes,
                    candidate,
                    current
                    ));
            }

            return entries;
        }

        /// <summary>Aplica el plan sobre las entidades: agrega las nuevas y actualiza las existentes. No guarda.</summary>
        public void Apply(IReadOnlyList<ProductImportEntry> plan)
        {
            foreach (var entry in plan)
            {
                if (entry.Action == ProductImportAction.Create)
                    _productRepository.Add(entry.Candidate!);
                else if (entry.Action == ProductImportAction.Update)
                {
                    var current = entry.Existing!;
                    var next = entry.Candidate!;
                    current.UpdateName(next.Name);
                    current.UpdateUnitOfMeasure(next.UnitOfMeasureCode);
                    current.UpdateIgvAffectation(next.IgvAffectation);
                    current.UpdateSalePrice(next.SalePrice);
                }
            }
        }

        private static List<ProductImportChangeDto> Diff(Product current, Product next, IReadOnlyDictionary<string, string> unitNames)
        {
            var changes = new List<ProductImportChangeDto>();
            if (current.Name != next.Name)
                changes.Add(new("Nombre", current.Name, next.Name));
            if (current.UnitOfMeasureCode != next.UnitOfMeasureCode)
                changes.Add(new("Unidad de medida", unitNames.GetValueOrDefault(current.UnitOfMeasureCode, current.UnitOfMeasureCode), unitNames[next.UnitOfMeasureCode]));
            if (current.IgvAffectation != next.IgvAffectation)
                changes.Add(new("Afectación IGV", current.IgvAffectation.Description, next.IgvAffectation.Description));
            if (current.SalePrice != next.SalePrice)
                changes.Add(new("Precio de venta", FormatPrice(current.SalePrice), FormatPrice(next.SalePrice)));
            return changes;
        }

        private static List<ProductImportChangeDto> Values(Product next, IReadOnlyDictionary<string, string> unitNames) =>
        [
            new("Unidad de medida", null, unitNames[next.UnitOfMeasureCode]),
            new("Afectación IGV", null, next.IgvAffectation.Description),
            new("Precio de venta", null, FormatPrice(next.SalePrice)),
        ];

        private static string FormatPrice(decimal price) => NumberText.Money(price);

        // Si falta la unidad, se usa esta solo para validar el resto de la fila (la fila igual queda con error).
        private const string PlaceholderUnitCode = "NIU";

        /// <summary>
        /// La unidad se reconoce por su nombre corto ("Docena"), su nombre SUNAT ("UNIDAD (BIENES)") o su código ("DZN"),
        /// sin distinguir mayúsculas ni tildes. Debe estar activa.
        /// </summary>
        private static UnitOfMeasure? ParseUnit(string? text, IReadOnlyCollection<UnitOfMeasure> units, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add("La unidad de medida es requerida.");
                return null;
            }

            var unit = units.FirstOrDefault(u => MatchesIgnoringAccents(text, u.Name))
                ?? units.FirstOrDefault(u => MatchesIgnoringAccents(text, u.SunatName) || MatchesIgnoringAccents(text, u.Code));

            if (unit is null)
            {
                errors.Add($"La unidad de medida '{text.Trim()}' no existe. Elige una de la lista.");
                return null;
            }

            if (!unit.IsActive)
            {
                errors.Add($"La unidad de medida '{unit.Name}' está desactivada. Actívala en Administración › Unidades de medida o elige otra de la lista.");
                return null;
            }

            return unit;
        }

        private static bool MatchesIgnoringAccents(string text, string value) =>
            string.Compare(text.Trim(), value, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;

        private static IgvAffectation? ParseIgvAffectation(string? text, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add("La afectación del IGV es requerida.");
                return null;
            }

            foreach (var igv in Enum.GetValues<IgvAffectation>())
                if (Matches(text, igv.Description) || Matches(text, igv.ToString()))
                    return igv;

            errors.Add($"La afectación del IGV '{text.Trim()}' no existe. Elige una de la lista.");
            return null;
        }

        private static decimal? ParsePrice(ProductSheetRow row, List<string> errors)
        {
            // La base guarda 6 decimales: redondear igual evita marcar como "cambio" un precio que ya está guardado.
            if (row.SalePrice is not null)
                return Math.Round(row.SalePrice.Value, 6, MidpointRounding.AwayFromZero);

            if (!string.IsNullOrWhiteSpace(row.SalePriceText) && row.SalePriceText.Contains(','))
                errors.Add($"El precio de venta '{row.SalePriceText.Trim()}' tiene coma. Usa punto para los decimales y no separes los miles con comas, por ejemplo 1500.50.");
            else if (!string.IsNullOrWhiteSpace(row.SalePriceText))
                errors.Add($"El precio de venta '{row.SalePriceText.Trim()}' no es un número.");
            else
                errors.Add("El precio de venta es requerido.");

            return null;
        }

        private static bool Matches(string text, string value) =>
            string.Equals(text.Trim(), value, StringComparison.CurrentCultureIgnoreCase);
    }

    internal sealed record ProductImportEntry(
        int RowNumber,
        string? Code,
        string? Name,
        ProductImportAction Action,
        IReadOnlyCollection<string> Errors,
        IReadOnlyCollection<ProductImportChangeDto> Changes,
        Product? Candidate,
        Product? Existing
        );
}
