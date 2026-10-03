using System.Security.Cryptography;
using System.Text;
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
    /// Valida con las mismas reglas de <see cref="Product"/> (los textos de la fila los lee <see cref="ProductImportRowParser"/>).
    /// No guarda nada: la vista previa y la confirmación usan este mismo plan, y la confirmación revisa con
    /// <see cref="VersionOf"/> que sea el mismo que se vio.
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
                var unit = ProductImportRowParser.Unit(row.UnitOfMeasure, units, errors);
                var igv = ProductImportRowParser.IgvAffectation(row.IgvAffectation, errors);
                var price = ProductImportRowParser.Price(row, errors);
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
        /// Huella del plan: qué se hará con cada fila y la versión de cada producto existente que toca. La vista previa la
        /// entrega y la confirmación la devuelve: si no coincide, alguien creó o editó esos productos después de revisar,
        /// y no se guarda algo distinto de lo que se vio.
        /// </summary>
        public string VersionOf(IReadOnlyList<ProductImportEntry> plan)
        {
            var text = string.Join('\n', plan.Select(e =>
                $"{e.RowNumber}|{e.Code}|{e.Action}|{(e.Existing is { } existing ? _productRepository.VersionOf(existing) : 0)}"));
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
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
