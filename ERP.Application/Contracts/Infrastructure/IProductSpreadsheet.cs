namespace ERP.Application.Contracts.Infrastructure
{
    /// <summary>
    /// Planilla de productos (Excel): plantilla de carga masiva, exportación y lectura del archivo subido.
    /// Solo traduce entre el archivo y filas de texto; las reglas (cuántas filas, qué decir si algo falla) se aplican
    /// en Application.
    /// </summary>
    public interface IProductSpreadsheet
    {
        /// <summary>
        /// Genera el archivo con las filas indicadas (vacío = plantilla). <paramref name="unitNames"/> son las unidades
        /// activas: forman la lista desplegable de la columna "Unidad de medida", en las primeras <paramref name="maxRows"/> filas.
        /// </summary>
        byte[] Write(IReadOnlyCollection<ProductSheetRow> rows, IReadOnlyCollection<string> unitNames, int maxRows);

        /// <summary>Lee el archivo subido. Si no tiene el formato de la plantilla o pasa de <paramref name="maxRows"/> filas, dice por qué.</summary>
        ProductSheetReadResult Read(Stream file, int maxRows);
    }

    /// <summary>Por qué no se pudo leer el archivo subido.</summary>
    public enum ProductSheetReadError
    {
        /// <summary>No es un Excel (.xlsx) o está dañado.</summary>
        Unreadable,
        /// <summary>No tiene la hoja de productos de la plantilla.</summary>
        MissingSheet,
        /// <summary>La cabecera no es la de la plantilla.</summary>
        WrongColumns,
        /// <summary>Tiene más filas de las permitidas.</summary>
        TooManyRows,
        /// <summary>Descomprimido ocupa demasiado (un archivo armado para llenar la memoria del servidor).</summary>
        TooLarge
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
        ProductSheetReadError? Error
        );
}
