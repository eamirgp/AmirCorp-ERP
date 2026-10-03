using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Catalogs;
using ERP.Domain.Partners;
using ERP.Domain.Products;
using ERP.Domain.Purchases;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.Purchases.CreatePurchase
{
    /// <summary>
    /// Revisa las líneas antes de registrar la compra, para avisar todos los errores juntos ("Línea 2: …"). Las reglas
    /// de una sola línea son las del dominio; aquí se agregan las que necesitan otros registros (códigos ya usados).
    /// </summary>
    internal sealed class PurchaseLinesChecker
    {
        private readonly IProductRepository _productRepository;

        public PurchaseLinesChecker(IProductRepository productRepository) => _productRepository = productRepository;

        /// <param name="products">Los productos existentes que eligen las líneas, por Id.</param>
        /// <param name="units">Las unidades de las líneas y la de los productos nuevos, por código.</param>
        public async Task<List<string>> CheckAsync(
            CreatePurchaseLineDto[] lines,
            InvoicePriceType invoicePriceType,
            BusinessPartner supplier,
            IReadOnlyDictionary<Guid, Product> products,
            IReadOnlyDictionary<string, UnitOfMeasure> units
            )
        {
            var errors = new List<string>();

            for (var i = 0; i < lines.Length; i++)
            {
                var lineNumber = i + 1;

                if (lines[i].ProductId is { } productId)
                {
                    if (!products.TryGetValue(productId, out var product))
                        errors.Add($"Línea {lineNumber}: El producto no existe.");
                    else if (Purchase.ProductError(product) is { } productError)
                        errors.Add($"Línea {lineNumber}: {productError}");
                }

                var code = lines[i].InvoiceUnitOfMeasureCode;
                var unit = units.GetValueOrDefault(UnitOfMeasure.NormalizeCode(code));
                if (UnitOfMeasure.UsableError(unit, code) is { } unitError)
                    errors.Add($"Línea {lineNumber}: {unitError}");
                // Cantidad, monto, unidades por caja y lo que resulta de ellos (que el costo no salga 0, que quepa).
                else if (PurchaseLine.AmountsError(
                    invoicePriceType,
                    lines[i].InvoiceIgvAffectation,
                    unit!,
                    lines[i].InvoiceQuantity,
                    lines[i].InvoiceAmount,
                    lines[i].ConversionFactor
                    ) is { } amountsError)
                    errors.Add($"Línea {lineNumber}: {amountsError}");
            }

            errors.AddRange(await CheckNewProductsAsync(lines, units));
            errors.AddRange(await CheckSupplierCodesAsync(lines, supplier, products));

            return errors;
        }

        /// <summary>
        /// Códigos del proveedor que la compra enlaza (a productos existentes o nuevos): no se repiten en la compra, no
        /// los usa otro producto de este proveedor, y un producto que ya tiene otro código de este proveedor no se cambia
        /// desde aquí (se corrige en Productos).
        /// </summary>
        private async Task<List<string>> CheckSupplierCodesAsync(CreatePurchaseLineDto[] lines, BusinessPartner supplier, IReadOnlyDictionary<Guid, Product> products)
        {
            var errors = new List<string>();
            var links = lines
                .Select((line, i) => (
                    Number: i + 1,
                    Product: line.ProductId is { } id ? products.GetValueOrDefault(id) : null,
                    Code: line.NewProduct?.SupplierCode ?? line.SupplierCode))
                .Where(x => x.Code is not null && (x.Product is not null || lines[x.Number - 1].NewProduct is not null))
                .Select(x => (x.Number, x.Product, Code: ProductSupplierCode.NormalizeCode(x.Code!)))
                .ToList();

            if (links.Count == 0)
                return errors;

            var inUse = await _productRepository.SupplierCodesInUseAsync(links.Select(l => (supplier.Id, l.Code)).ToArray());

            foreach (var (number, product, code) in links)
            {
                if (product?.SupplierCodeError(supplier, code) is { } linkError)
                    errors.Add($"Línea {number}: {linkError}");
                else if (inUse.FirstOrDefault(c => c.Code == code && c.ProductCode != product?.Code) is { } used)
                    errors.Add($"Línea {number}: El código {code} de {supplier.Name} ya está en el producto {used.ProductCode} · {used.ProductName}. Elígelo de la lista.");
                else if (links.Count(l => l.Code == code) > 1)
                    errors.Add($"Línea {number}: El código {code} de {supplier.Name} está en más de una línea de esta compra.");
            }

            return errors;
        }

        /// <summary>
        /// Reglas de los productos nuevos que necesitan datos: el código interno no lo usa otro producto ni otra línea,
        /// y la unidad en que se cuenta su stock está activa.
        /// </summary>
        private async Task<List<string>> CheckNewProductsAsync(CreatePurchaseLineDto[] lines, IReadOnlyDictionary<string, UnitOfMeasure> units)
        {
            var errors = new List<string>();
            var news = lines.Select((line, i) => (Line: line, Number: i + 1)).Where(x => x.Line.NewProduct is not null).ToList();
            if (news.Count == 0)
                return errors;

            var codes = news.Select(x => Product.NormalizeCode(x.Line.NewProduct!.Code)).ToArray();
            var taken = (await _productRepository.GetByCodesAsync(codes.Distinct().ToArray())).ToDictionary(p => p.Code);

            foreach (var (line, number) in news)
            {
                var code = Product.NormalizeCode(line.NewProduct!.Code);

                if (taken.TryGetValue(code, out var existing))
                    errors.Add($"Línea {number}: {Product.CodeTakenError(existing)}");
                else if (codes.Count(c => c == code) > 1)
                    errors.Add($"Línea {number}: El código interno {code} está en más de un producto nuevo de esta compra.");
            }

            // Los productos nuevos se cuentan en unidades: esa unidad tiene que estar activa.
            if (UnitOfMeasure.UsableError(units.GetValueOrDefault(UnitOfMeasure.BaseUnitCode), UnitOfMeasure.BaseUnitCode) is { } unitError)
                errors.Add($"Los productos nuevos se cuentan en unidades: {unitError}");

            return errors;
        }
    }
}
