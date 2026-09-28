namespace ERP.Application.Features.Products.ProductImport
{
    /// <summary>Archivo subido y modo de importación.</summary>
    public sealed record ImportProductsDto(
        Stream File,
        bool UpdateExisting
        );
}
