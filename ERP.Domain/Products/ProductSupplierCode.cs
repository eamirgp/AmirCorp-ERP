using ERP.Domain.Common;

namespace ERP.Domain.Products
{
    /// <summary>
    /// Código con el que un proveedor identifica al producto (el de su factura, proforma o catálogo).
    /// Cada proveedor tiene su propio código; el producto se sigue identificando por su código interno.
    /// </summary>
    public sealed class ProductSupplierCode : BaseEntity
    {
        public const int CodeMaxLength = 50;

        public Guid ProductId { get; }
        public Guid SupplierId { get; }
        public string Code { get; private set; }

        private ProductSupplierCode(Guid id, Guid productId, Guid supplierId, string code) : base(id)
        {
            ProductId = productId;
            SupplierId = supplierId;
            Code = code;
        }

        internal static ProductSupplierCode Create(Guid productId, Guid supplierId, string code) =>
            new(Guid.CreateVersion7(), productId, supplierId, code);

        internal void UpdateCode(string code) =>
            Code = code;

        public static string NormalizeCode(string code) =>
            code.Trim().ToUpperInvariant();
    }
}
