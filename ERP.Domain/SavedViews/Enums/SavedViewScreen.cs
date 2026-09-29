namespace ERP.Domain.SavedViews.Enums
{
    /// <summary>Pantallas de lista que admiten vistas guardadas.</summary>
    public enum SavedViewScreen
    {
        Products = 1,
        // Clientes y proveedores son el mismo registro, pero cada lista tiene sus propias vistas.
        Suppliers = 2,
        Purchases = 3,
        Companies = 4,
        Users = 5,
        Audit = 6,
        Clients = 7
    }
}
