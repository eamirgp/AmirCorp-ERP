namespace ERP.Application.Features.Products.ProductImport
{
    /// <summary>
    /// Un dato de la fila, con los valores ya listos para mostrar. En una actualización, From es el valor actual;
    /// en un producto nuevo es null y To es el valor que se va a crear.
    /// </summary>
    public sealed record ProductImportChangeDto(
        string Field,
        string? From,
        string To
        );
}
