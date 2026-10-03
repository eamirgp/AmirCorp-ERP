using System.Globalization;
using ERP.Application.Common.Formatting;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Catalogs;
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
                .Select(r => Product.NormalizeCode(r.Code!))
                .Distinct()
                .ToArray();

            var existing = (await _productRepository.GetByCodesAsync(codes)).ToDictionary(p => p.Code);
            var firstRowByCode = new Dictionary<string, int>();
            var entries = new List<ProductImportEntry>(rows.Count);

            foreach (var row in rows)
            {
                // Las mismas reglas del dominio, todas juntas: la fila muestra todo lo que tiene mal de una vez.
                var errors = new List<string>();
                var codeError = Product.CodeError(row.Code);
                if (codeError is not null)
                    errors.Add(codeError);
                if (Product.NameError(row.Name) is { } nameError)
                    errors.Add(nameError);
                var unit = ParseUnit(row.UnitOfMeasure, units, errors);
                var igv = ParseIgvAffectation(row.IgvAffectation, errors);
                var price = ParsePrice(row, errors);
                if (price is not null && Product.SalePriceError(price) is { } priceError)
                    errors.Add(priceError);

                var code = codeError is null ? Product.NormalizeCode(row.Code!) : row.Code?.Trim();

                if (codeError is null)
                {
                    if (firstRowByCode.TryGetValue(code!, out var firstRow))
                        errors.Add($"El código interno {code} está repetido: ya aparece en la fila {firstRow}.");
                    else
                        firstRowByCode[code!] = row.RowNumber;
                }

                if (errors.Count > 0)
                {
                    entries.Add(new ProductImportEntry(row.RowNumber, code, row.Name?.Trim(), ProductImportAction.Error, errors, [], null, null, null));
                    continue;
                }

                var candidate = Product.Create(row.Code!, row.Name!, unit!, igv!.Value, price!.Value);

                if (!existing.TryGetValue(candidate.Code, out var current))
                {
                    // Se muestran los valores que se crearán, para detectar antes de guardar un precio o unidad mal leídos.
                    entries.Add(new ProductImportEntry(row.RowNumber, code, candidate.Name, ProductImportAction.Create, [], Values(candidate, unitNames), candidate, null, unit));
                    continue;
                }

                if (!updateExisting)
                {
                    entries.Add(new ProductImportEntry(row.RowNumber, code, candidate.Name, ProductImportAction.Skip, [], [], null, null, null));
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
                    current,
                    unit
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
                    current.UpdateUnitOfMeasure(entry.Unit!);
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

        /// <summary>
        /// La unidad se reconoce por su nombre corto ("Docena"), su nombre SUNAT ("UNIDAD (BIENES)") o su código ("DZN"),
        /// sin distinguir mayúsculas ni tildes (<see cref="UnitOfMeasure.IsKnownAs"/>). Debe estar activa. Si el texto
        /// sirve para más de una activa, no se adivina: la fila queda con error, para no cambiar la unidad sin que se note.
        /// </summary>
        private static UnitOfMeasure? ParseUnit(string? text, IReadOnlyCollection<UnitOfMeasure> units, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add("La unidad de medida es requerida.");
                return null;
            }

            var matches = units.Where(u => u.IsKnownAs(text)).ToList();
            var active = matches.Where(u => u.IsActive).ToList();

            if (active.Count > 1)
            {
                errors.Add($"La unidad de medida '{text.Trim()}' puede ser {string.Join(" o ", active.Select(u => $"{u.Name} ({u.Code})"))}. Escribe su código.");
                return null;
            }

            var unit = active.FirstOrDefault() ?? matches.FirstOrDefault();
            if (unit is null)
            {
                errors.Add($"La unidad de medida '{text.Trim()}' no existe. Elige una de la lista.");
                return null;
            }

            if (UnitOfMeasure.UsableError(unit, text) is { } unitError)
            {
                errors.Add(unitError);
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

            // Igual que la unidad: sin distinguir mayúsculas ni tildes ("Operacion" es "Operación").
            foreach (var igv in Enum.GetValues<IgvAffectation>())
                if (MatchesIgnoringAccents(text, igv.Description) || MatchesIgnoringAccents(text, igv.ToString()))
                    return igv;

            errors.Add($"La afectación del IGV '{text.Trim()}' no existe. Elige una de la lista.");
            return null;
        }

        private static decimal? ParsePrice(ProductSheetRow row, List<string> errors)
        {
            // Con más de 2 decimales la fila queda con error (Product.SalePriceError): no se redondea sin avisar.
            if (row.SalePrice is not null)
                return row.SalePrice.Value;

            if (!string.IsNullOrWhiteSpace(row.SalePriceText) && row.SalePriceText.Contains(','))
                errors.Add($"El precio de venta '{row.SalePriceText.Trim()}' tiene coma. Usa punto para los decimales y no separes los miles con comas, por ejemplo 1500.50.");
            else if (!string.IsNullOrWhiteSpace(row.SalePriceText))
                errors.Add($"El precio de venta '{row.SalePriceText.Trim()}' no es un número.");
            else
                errors.Add("El precio de venta es requerido.");

            return null;
        }
    }

    internal sealed record ProductImportEntry(
        int RowNumber,
        string? Code,
        string? Name,
        ProductImportAction Action,
        IReadOnlyCollection<string> Errors,
        IReadOnlyCollection<ProductImportChangeDto> Changes,
        Product? Candidate,
        Product? Existing,
        // La unidad de la fila, para actualizar el producto existente.
        UnitOfMeasure? Unit
        );
}
