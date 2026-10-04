namespace ERP.Domain.Products
{
    /// <summary>
    /// Un problema de la lista de códigos de proveedores de un producto (<see cref="Product.SupplierCodesErrors"/>): de qué
    /// proveedor es y si es de su código (mal escrito) o del proveedor en sí (repetido, bloqueado, no es proveedor).
    /// </summary>
    public sealed record SupplierCodeProblem(Guid SupplierId, string Message, bool IsAboutCode = false);
}
