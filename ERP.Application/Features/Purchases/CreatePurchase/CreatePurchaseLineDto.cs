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
        // Unidades por cada unidad de la factura. Solo con unidades variables (Caja); con fijas la pone el catálogo.
        decimal? ConversionFactor,
        // Con un producto ya registrado: el código con que lo vende el proveedor de esta compra, para enlazarlo al
        // producto si todavía no lo tiene. (Un producto nuevo lo trae en NewProduct.)
        string? SupplierCode
        );

    /// <summary>
    /// Producto que todavía no existe: se registra junto con la compra. La afectación al IGV sale de la línea, se
    /// cuenta en unidades (lo comprado por caja o docena se convierte) y nace sin precio de venta.
    /// </summary>
    /// <param name="Code">Código interno. Por defecto la pantalla propone el mismo de la factura.</param>
    /// <param name="SupplierCode">Código con que lo vende el proveedor de esta compra, si la factura lo trae.</param>
    public sealed record CreatePurchaseNewProductDto(string Code, string Name, string? SupplierCode);
}
