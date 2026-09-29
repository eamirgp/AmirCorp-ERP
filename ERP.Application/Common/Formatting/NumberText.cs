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

        public static string Money(decimal value) => "S/ " + Group(value.ToString("#,##0.00", CultureInfo.InvariantCulture));

        public static string Decimal(decimal value) => Group(value.ToString("#,##0.######", CultureInfo.InvariantCulture));

        public static string Integer(int value) => Group(value.ToString("#,##0", CultureInfo.InvariantCulture));

        private static string Group(string invariant) => invariant.Replace(',', ThousandsSeparator);
    }
}
