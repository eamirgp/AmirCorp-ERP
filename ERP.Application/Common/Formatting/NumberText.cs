using System.Globalization;

namespace ERP.Application.Common.Formatting
{
    /// <summary>
    /// Números como texto para mostrar, con el mismo formato que la pantalla: punto para los decimales y un espacio
    /// para los miles ("S/ 1 234.50"), sin comas, para que nadie confunda comas con puntos.
    /// </summary>
    public static class NumberText
    {
        /// <summary>Espacio que no se corta entre líneas (U+00A0), el mismo que usa la pantalla.</summary>
        public const char ThousandsSeparator = ' ';

        /// <summary>
        /// Monto en soles con dos decimales. Si tiene más (un precio guardado antes de la regla de 2 decimales), se
        /// muestran: redondearlo escondería un cambio en el historial o en la vista previa del Excel.
        /// </summary>
        public static string Money(decimal value) => "S/ " + Group(value.ToString("#,##0.00####", CultureInfo.InvariantCulture));

        public static string Decimal(decimal value) => Group(value.ToString("#,##0.######", CultureInfo.InvariantCulture));

        /// <summary>Costo unitario: dos decimales como un monto, pero sin perder los de un costo muy pequeño (0.004237).</summary>
        public static string Cost(decimal value) => Group(value.ToString("#,##0.00####", CultureInfo.InvariantCulture));

        public static string Integer(int value) => Group(value.ToString("#,##0", CultureInfo.InvariantCulture));

        private static string Group(string invariant) => invariant.Replace(',', ThousandsSeparator);
    }
}
