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

        /// <summary>Qué tiene de malo el código, o null si está bien.</summary>
        public static string? CodeError(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "Escribe el código del proveedor.";

            if (NormalizeCode(code).Length > CodeMaxLength)
                return $"El código del proveedor no puede exceder los {CodeMaxLength} caracteres.";

            // Un salto de línea o una tabulación vienen de pegar o de una celda de Excel, nunca de la factura.
            if (NormalizeCode(code).Any(char.IsControl))
                return "El código del proveedor no puede tener saltos de línea ni tabulaciones.";

            return null;
        }

        /// <summary>
        /// El aviso de "código del proveedor ocupado", el mismo en Productos y en una compra: dice de qué producto es, y si
        /// está desactivado (entonces no aparece en las listas y hay que activarlo para usarlo).
        /// </summary>
        public static string CodeTakenError(string code, string supplierName, string productCode, string productName, bool productIsActive) =>
            $"El código {code} de {supplierName} ya es del producto {productCode} · {productName}"
            + (productIsActive ? "." : ", que está desactivado. Actívalo en Productos para usarlo.");
    }
}
