namespace ERP.Application.Features.Products.ProductImport
{
    /// <summary>Archivo subido y modo de importación.</summary>
    /// <param name="PlanVersion">
    /// Al confirmar: la huella que entregó la vista previa (<see cref="ProductImportPreviewDto.PlanVersion"/>), para
    /// guardar exactamente lo que se revisó. En la vista previa no se usa.
    /// </param>
    public sealed record ImportProductsDto(
        Stream File,
        bool UpdateExisting,
        string? PlanVersion = null
        );
}
