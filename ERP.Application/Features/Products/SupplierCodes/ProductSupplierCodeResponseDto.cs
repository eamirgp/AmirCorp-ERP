namespace ERP.Application.Features.Products.SupplierCodes
{
    /// <summary>Código de un proveedor para el producto, con los datos del proveedor para mostrarlo.</summary>
    public sealed record ProductSupplierCodeResponseDto(
        Guid SupplierId,
        string SupplierName,
        string SupplierDocumentNumber,
        string Code
        );
}
