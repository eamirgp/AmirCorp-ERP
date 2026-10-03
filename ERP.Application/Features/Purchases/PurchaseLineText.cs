using ERP.Application.Common.Formatting;

namespace ERP.Application.Features.Purchases
{
    /// <summary>Textos de una línea de compra, iguales en la vista previa y en el detalle de la compra registrada.</summary>
    public static class PurchaseLineText
    {
        /// <summary>Lo que entra al inventario: "120 und. (24 por caja) · costo 5.00 c/u". Sin conversión, solo las unidades.</summary>
        public static string Inventory(decimal inventoryQuantity, decimal conversionFactor, string invoiceUnitName, decimal inventoryUnitCost) =>
            NumberText.Decimal(inventoryQuantity) + " und."
            + (conversionFactor == 1 ? "" : $" ({NumberText.Decimal(conversionFactor)} por {invoiceUnitName.ToLowerInvariant()})")
            + " · costo " + NumberText.Cost(inventoryUnitCost) + " c/u";
    }
}
