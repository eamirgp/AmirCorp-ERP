namespace ERP.Application.Features.Products.ProductImport
{
    /// <summary>Qué pasará con cada fila si se confirma la importación. No guarda nada.</summary>
    public sealed record ProductImportPreviewDto(
        IReadOnlyCollection<ProductImportRowDto> Rows,
        int ToCreate,
        int ToUpdate,
        int Skipped,
        int Unchanged,
        int WithErrors
        )
    {
        /// <summary>Productos que se guardarán al confirmar: los nuevos más los actualizados.</summary>
        public int ToImport => ToCreate + ToUpdate;

        /// <summary>Se puede confirmar si no hay errores y hay al menos un producto para crear o actualizar.</summary>
        public bool CanImport => WithErrors == 0 && ToImport > 0;
    }
}
