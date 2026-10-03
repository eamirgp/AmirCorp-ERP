using ERP.Application.Common.Formatting;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Purchases
{
    /// <summary>Textos de una línea de compra, iguales en la vista previa y en el detalle de la compra registrada.</summary>
    public static class PurchaseLineText
    {
        /// <summary>
        /// Lo que entra al inventario: "120 und. (24 por caja) · costo US$ 5.00 c/u". Sin conversión, solo las unidades.
        /// El costo va con su moneda (la de la factura): en una compra en dólares, "5.00" solo se leería como soles.
        /// </summary>
        /// <param name="currency">La moneda de la compra; null si todavía no se eligió (vista previa).</param>
        public static string Inventory(decimal inventoryQuantity, decimal conversionFactor, string invoiceUnitName, decimal inventoryUnitCost, Currency? currency) =>
            NumberText.Decimal(inventoryQuantity) + " und."
            + (conversionFactor == 1 ? "" : $" ({NumberText.Decimal(conversionFactor)} por {invoiceUnitName.ToLowerInvariant()})")
            + " · costo " + (currency is { } c && Enum.IsDefined(c) ? c.Symbol + " " : "") + NumberText.Cost(inventoryUnitCost) + " c/u";
    }
}
