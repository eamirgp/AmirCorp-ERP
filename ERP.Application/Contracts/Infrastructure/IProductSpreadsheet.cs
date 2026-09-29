namespace ERP.Application.Contracts.Infrastructure
{
    /// <summary>
    /// Planilla de productos (Excel): plantilla de carga masiva, exportación y lectura del archivo subido.
    /// Solo traduce entre el archivo y filas de texto; las reglas de negocio se aplican en Application.
    /// </summary>
    public interface IProductSpreadsheet
    {
        /// <summary>
        /// Genera el archivo con las filas indicadas (vacío = plantilla). <paramref name="unitNames"/> son las unidades
        /// activas: forman la lista desplegable de la columna "Unidad de medida".
        /// </summary>
        byte[] Write(IReadOnlyCollection<ProductSheetRow> rows, IReadOnlyCollection<string> unitNames);

        /// <summary>Lee el archivo subido. Si no tiene el formato de la plantilla, devuelve el error.</summary>
        ProductSheetReadResult Read(Stream file);
    }

    /// <summary>
    /// Una fila de la planilla tal como está escrita. Unidad y afectación vienen como texto
    /// (la descripción que el usuario eligió de la lista); el precio, como número si la celda lo es.
    /// </summary>
    public sealed record ProductSheetRow(
        int RowNumber,
        string? Code,
        string? Name,
        string? UnitOfMeasure,
        string? IgvAffectation,
        decimal? SalePrice,
        string? SalePriceText
        );

    public sealed record ProductSheetReadResult(
        IReadOnlyList<ProductSheetRow> Rows,
        string? Error
        );
}
