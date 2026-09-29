namespace ERP.Application.Features.Products.SupplierCodes
{
    /// <summary>Código con el que un proveedor identifica al producto, tal como llega del formulario.</summary>
    public sealed record ProductSupplierCodeDto(Guid SupplierId, string Code);
}
