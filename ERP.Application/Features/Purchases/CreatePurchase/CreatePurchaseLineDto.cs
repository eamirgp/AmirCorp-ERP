using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Purchases.CreatePurchase
{
    public sealed record CreatePurchaseLineDto(
        // El producto: uno ya registrado (ProductId) o uno nuevo que se registra con la compra (NewProduct).
        Guid? ProductId,
        CreatePurchaseNewProductDto? NewProduct,
        IgvAffectation InvoiceIgvAffectation,
        string InvoiceUnitOfMeasureCode,
        decimal InvoiceQuantity,
        decimal InvoiceAmount,
        decimal ConversionFactor
        );

    /// <summary>
    /// Producto que todavía no existe: se registra junto con la compra. La afectación al IGV sale de la línea, y la
    /// unidad también salvo que se indique otra para el inventario. Nace sin precio de venta.
    /// </summary>
    /// <param name="Code">Código interno. Por defecto la pantalla propone el mismo de la factura.</param>
    /// <param name="SupplierCode">Código con que lo vende el proveedor de esta compra, si la factura lo trae.</param>
    /// <param name="UnitOfMeasureCode">Unidad en que se lleva su inventario; null si es la misma de la línea.</param>
    public sealed record CreatePurchaseNewProductDto(string Code, string Name, string? SupplierCode, string? UnitOfMeasureCode);
}
