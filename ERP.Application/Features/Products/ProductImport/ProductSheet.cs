using ERP.Application.Common.Formatting;
using ERP.Application.Contracts.Infrastructure;

namespace ERP.Application.Features.Products.ProductImport
{
    /// <summary>
    /// Reglas y textos de la planilla de productos (decisión 11): cuántas filas admite, qué decir si el archivo no se
    /// puede leer y cómo se llaman los archivos que se descargan. Infrastructure solo lee y escribe el Excel.
    /// </summary>
    internal static class ProductSheet
    {
        /// <summary>Filas de productos por archivo: la carga y la exportación usan el mismo límite.</summary>
        public const int MaxRows = 5000;

        public const string TemplateFileName = "plantilla-productos.xlsx";

        /// <summary>"productos-2026-10-03.xlsx", o "productos-filtrados-…" si se exportó solo lo filtrado.</summary>
        public static string ExportFileName(bool filtered, DateOnly today) =>
            $"{(filtered ? "productos-filtrados" : "productos")}-{today:yyyy-MM-dd}.xlsx";

        public static string ReadErrorMessage(ProductSheetReadError error) =>
            error switch
            {
                ProductSheetReadError.MissingSheet => "El archivo no tiene la hoja \"Productos\". Descarga la plantilla y copia tus datos en ella.",
                ProductSheetReadError.WrongColumns => "Las columnas no coinciden con la plantilla. Descarga la plantilla y copia tus datos en ella, sin cambiar la cabecera.",
                ProductSheetReadError.TooManyRows => $"El archivo tiene más de {NumberText.Integer(MaxRows)} filas. Divídelo en varios archivos.",
                ProductSheetReadError.TooLarge => "El archivo es demasiado grande para leerlo. Revisa que sea la plantilla de productos y divídelo en varios archivos.",
                _ => "No se pudo leer el archivo. Sube la plantilla en formato Excel (.xlsx)."
            };

        public static string TooManyToExport(int count) =>
            $"Hay {NumberText.Integer(count)} productos y la planilla admite hasta {NumberText.Integer(MaxRows)} por archivo (para poder volver a subirla). Filtra la lista para exportar menos.";
    }
}
