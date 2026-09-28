using System.Globalization;
using ClosedXML.Excel;
using ERP.Application.Contracts.Infrastructure;
using ERP.Domain.Catalogs;

namespace ERP.Infrastructure.Services.Spreadsheets
{
    /// <summary>
    /// Plantilla de productos en Excel (ClosedXML): la hoja "Productos" con listas desplegables para la unidad
    /// y la afectación del IGV, una hoja de instrucciones y una hoja oculta con las listas.
    /// </summary>
    internal sealed class ProductSpreadsheet : IProductSpreadsheet
    {
        private const string DataSheet = "Productos";
        private const string ListsSheet = "Listas";
        private const string HelpSheet = "Instrucciones";
        private const int MaxRows = 5000;

        private static readonly string[] Headers = ["Código", "Nombre", "Unidad de medida", "Afectación IGV", "Precio de venta (S/, con IGV)"];

        public byte[] Write(IReadOnlyCollection<ProductSheetRow> rows)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add(DataSheet);
            var lists = workbook.Worksheets.Add(ListsSheet);
            var help = workbook.Worksheets.Add(HelpSheet);

            // Cabecera
            for (var c = 0; c < Headers.Length; c++)
                sheet.Cell(1, c + 1).Value = Headers[c];

            var header = sheet.Range(1, 1, 1, Headers.Length);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
            header.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            sheet.SheetView.FreezeRows(1);

            sheet.Column(1).Width = 18;
            sheet.Column(2).Width = 48;
            sheet.Column(3).Width = 20;
            sheet.Column(4).Width = 30;
            sheet.Column(5).Width = 28;

            // Código siempre como texto: así Excel no convierte "00123" en 123.
            sheet.Column(1).Style.NumberFormat.Format = "@";
            sheet.Column(5).Style.NumberFormat.Format = "#,##0.00";

            // Listas de valores válidos (hoja oculta) y validación en las celdas.
            var units = Enum.GetValues<UnitOfMeasure>().Select(u => u.Description).ToArray();
            var igvs = Enum.GetValues<IgvAffectation>().Select(i => i.Description).ToArray();
            for (var i = 0; i < units.Length; i++) lists.Cell(i + 1, 1).Value = units[i];
            for (var i = 0; i < igvs.Length; i++) lists.Cell(i + 1, 2).Value = igvs[i];
            lists.Visibility = XLWorksheetVisibility.Hidden;

            var lastDataRow = MaxRows + 1;
            AddList(sheet.Range(2, 3, lastDataRow, 3), lists.Range(1, 1, units.Length, 1), "Unidad de medida");
            AddList(sheet.Range(2, 4, lastDataRow, 4), lists.Range(1, 2, igvs.Length, 2), "Afectación IGV");

            var price = sheet.Range(2, 5, lastDataRow, 5).CreateDataValidation();
            price.Decimal.EqualOrGreaterThan(0);
            price.ErrorTitle = "Precio de venta";
            price.ErrorMessage = "Escribe un número mayor o igual a cero.";
            price.ShowErrorMessage = true;

            // Filas (exportación)
            foreach (var row in rows)
            {
                sheet.Cell(row.RowNumber, 1).Value = row.Code ?? "";
                sheet.Cell(row.RowNumber, 2).Value = row.Name ?? "";
                sheet.Cell(row.RowNumber, 3).Value = row.UnitOfMeasure ?? "";
                sheet.Cell(row.RowNumber, 4).Value = row.IgvAffectation ?? "";
                if (row.SalePrice is not null)
                    sheet.Cell(row.RowNumber, 5).Value = row.SalePrice.Value;
            }

            WriteHelp(help, units, igvs);
            sheet.SetTabActive();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public ProductSheetReadResult Read(Stream file)
        {
            XLWorkbook workbook;
            try
            {
                workbook = new XLWorkbook(file);
            }
            catch
            {
                return Fail("No se pudo leer el archivo. Sube la plantilla en formato Excel (.xlsx).");
            }

            using (workbook)
            {
                if (!workbook.Worksheets.TryGetWorksheet(DataSheet, out var sheet))
                    return Fail($"El archivo no tiene la hoja \"{DataSheet}\". Descarga la plantilla y copia tus datos en ella.");

                for (var c = 0; c < Headers.Length; c++)
                {
                    var text = TextOf(sheet.Cell(1, c + 1));
                    if (!string.Equals(text, Headers[c], StringComparison.CurrentCultureIgnoreCase))
                        return Fail("Las columnas no coinciden con la plantilla. Descarga la plantilla y copia tus datos en ella, sin cambiar la cabecera.");
                }

                var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
                if (lastRow - 1 > MaxRows)
                    return Fail($"El archivo tiene más de {MaxRows:N0} filas. Divídelo en varios archivos.");

                var rows = new List<ProductSheetRow>();
                for (var r = 2; r <= lastRow; r++)
                {
                    var code = TextOf(sheet.Cell(r, 1));
                    var name = TextOf(sheet.Cell(r, 2));
                    var unit = TextOf(sheet.Cell(r, 3));
                    var igv = TextOf(sheet.Cell(r, 4));
                    var (price, priceText) = PriceOf(sheet.Cell(r, 5));

                    // Las filas completamente vacías se ignoran.
                    if (code is null && name is null && unit is null && igv is null && price is null && priceText is null)
                        continue;

                    rows.Add(new ProductSheetRow(r, code, name, unit, igv, price, priceText));
                }

                return new ProductSheetReadResult(rows, null);
            }
        }

        private static ProductSheetReadResult Fail(string error) => new([], error);

        private static void AddList(IXLRange cells, IXLRange options, string title)
        {
            var validation = cells.CreateDataValidation();
            validation.List(options, true);
            validation.ErrorTitle = title;
            validation.ErrorMessage = "Elige un valor de la lista.";
            validation.ShowErrorMessage = true;
        }

        /// <summary>Valor de la celda. Si tiene una fórmula se usa el último valor calculado por Excel, sin ejecutarla.</summary>
        private static XLCellValue ValueOf(IXLCell cell) => cell.HasFormula ? cell.CachedValue : cell.Value;

        private static string? TextOf(IXLCell cell)
        {
            var value = ValueOf(cell);
            if (value.IsBlank) return null;

            var text = value.IsNumber
                ? value.GetNumber().ToString(CultureInfo.InvariantCulture)
                : value.ToString(CultureInfo.InvariantCulture);

            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        private static (decimal? Price, string? Text) PriceOf(IXLCell cell)
        {
            var value = ValueOf(cell);
            if (value.IsBlank) return (null, null);
            if (value.IsNumber) return ((decimal)Math.Round(value.GetNumber(), 6), null);

            var text = TextOf(cell);
            if (text is null) return (null, null);

            // Acepta "25.50" y también "25,50" (sin separador de miles).
            var normalized = text.Contains(',') && !text.Contains('.') ? text.Replace(',', '.') : text;
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                ? (parsed, null)
                : (null, text);
        }

        private static void WriteHelp(IXLWorksheet help, string[] units, string[] igvs)
        {
            string[] lines =
            [
                "Carga masiva de productos",
                "",
                "1. Llena la hoja \"Productos\": una fila por producto, desde la fila 2.",
                "2. No cambies ni borres la fila de títulos.",
                "3. Código: único para cada producto. Se guarda en mayúsculas.",
                "4. Unidad de medida y Afectación IGV: elígelos de la lista desplegable de cada celda.",
                "5. Precio de venta: en soles e incluye IGV. Usa números, por ejemplo 25.50",
                "6. Sube el archivo en el sistema: antes de guardar verás qué productos se crean, cuáles se actualizan y los errores.",
                "",
                "Si el código ya existe, el producto se omite. Para actualizar productos existentes (por ejemplo, sus precios),",
                "marca la opción \"Actualizar los productos que ya existen\" al subir el archivo.",
                "",
                "Si una sola fila tiene errores no se guarda nada: corrige el archivo y vuelve a subirlo.",
                "",
                "Unidades de medida: " + string.Join(", ", units),
                "Afectación IGV: " + string.Join(", ", igvs),
                "",
                "Ejemplo:",
            ];

            for (var i = 0; i < lines.Length; i++)
                help.Cell(i + 1, 1).Value = lines[i];

            help.Cell(1, 1).Style.Font.Bold = true;
            help.Cell(1, 1).Style.Font.FontSize = 14;

            var exampleRow = lines.Length + 1;
            for (var c = 0; c < Headers.Length; c++)
                help.Cell(exampleRow, c + 1).Value = Headers[c];
            help.Range(exampleRow, 1, exampleRow, Headers.Length).Style.Font.Bold = true;

            help.Cell(exampleRow + 1, 1).Value = "EL-1003";
            help.Cell(exampleRow + 1, 2).Value = "Power bank 20 000 mAh carga rápida";
            help.Cell(exampleRow + 1, 3).Value = units[0];
            help.Cell(exampleRow + 1, 4).Value = igvs[0];
            help.Cell(exampleRow + 1, 5).Value = 89.90m;

            help.Column(1).Width = 18;
            help.Column(2).Width = 40;
            help.Column(3).Width = 18;
            help.Column(4).Width = 28;
            help.Column(5).Width = 26;
        }
    }
}
