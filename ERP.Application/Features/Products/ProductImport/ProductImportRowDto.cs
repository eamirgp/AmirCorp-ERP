namespace ERP.Application.Features.Products.ProductImport
{
    public sealed record ProductImportRowDto(
        int RowNumber,
        string? Code,
        string? Name,
        ProductImportAction Action,
        IReadOnlyCollection<string> Errors,
        IReadOnlyCollection<ProductImportChangeDto> Changes
        )
    {
        public string ActionDescription => Action switch
        {
            ProductImportAction.Create => "Se creará",
            ProductImportAction.Update => "Se actualizará",
            ProductImportAction.Skip => "Se omitirá: ya existe",
            ProductImportAction.Unchanged => "Sin cambios",
            _ => "Con errores"
        };
    }
}
