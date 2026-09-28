namespace ERP.Application.Features.Products.ProductImport
{
    /// <summary>Un dato que cambia al actualizar un producto existente, con los valores ya listos para mostrar.</summary>
    public sealed record ProductImportChangeDto(
        string Field,
        string From,
        string To
        );
}
