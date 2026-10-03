using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners;
using ERP.Domain.Products;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.Purchases.CreatePurchase
{
    /// <summary>
    /// El producto de cada línea, ya revisada: el existente (enlazándole el código de la factura si se pidió) o el nuevo,
    /// que se registra con la compra en la misma transacción.
    /// </summary>
    internal sealed class PurchaseLineProducts
    {
        private readonly IProductRepository _productRepository;

        public PurchaseLineProducts(IProductRepository productRepository) => _productRepository = productRepository;

        /// <returns>Un producto por línea, en el mismo orden.</returns>
        public Product[] Resolve(CreatePurchaseLineDto[] lines, BusinessPartner supplier, IReadOnlyDictionary<Guid, Product> products)
        {
            var result = new Product[lines.Length];

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].NewProduct is not { } newProduct)
                {
                    var existing = products[lines[i].ProductId!.Value];
                    // El código de esta factura queda enlazado al producto: la próxima compra a este proveedor lo encuentra.
                    if (lines[i].SupplierCode is { } linkedCode)
                        existing.AddSupplierCode(supplier, linkedCode);
                    result[i] = existing;
                    continue;
                }

                // Toma la afectación al IGV de su línea, se cuenta en unidades (lo comprado por caja o docena se convierte
                // con las unidades por caja) y nace sin precio de venta.
                var created = Product.Create(
                    newProduct.Code,
                    newProduct.Name,
                    UnitOfMeasure.BaseUnitCode,
                    lines[i].InvoiceIgvAffectation,
                    salePrice: 0);

                if (newProduct.SupplierCode is { } supplierCode)
                    created.AddSupplierCode(supplier, supplierCode);

                _productRepository.Add(created);
                result[i] = created;
            }

            return result;
        }
    }
}
