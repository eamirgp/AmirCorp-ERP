using System.Globalization;
using ERP.Application.Contracts.Infrastructure;
using ERP.Domain.Catalogs;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.Products.ProductImport
{
    /// <summary>
    /// Lee los textos de una fila de la planilla (unidad, afectación, precio) y los convierte en datos del dominio. Si
    /// un texto no se entiende, agrega a la fila el mensaje de por qué, sin adivinar.
    /// </summary>
    internal static class ProductImportRowParser
    {
        /// <summary>
        /// La unidad se reconoce por su nombre corto ("Docena"), su nombre SUNAT ("UNIDAD (BIENES)") o su código ("DZN"),
        /// sin distinguir mayúsculas ni tildes (<see cref="UnitOfMeasure.IsKnownAs"/>). Debe estar activa. Si el texto
        /// sirve para más de una activa, no se adivina: la fila queda con error, para no cambiar la unidad sin que se note.
        /// </summary>
        public static UnitOfMeasure? Unit(string? text, IReadOnlyCollection<UnitOfMeasure> units, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add("La unidad de medida es requerida.");
                return null;
            }

            var matches = units.Where(u => u.IsKnownAs(text)).ToList();
            var active = matches.Where(u => u.IsActive).ToList();

            if (active.Count > 1)
            {
                errors.Add($"La unidad de medida '{text.Trim()}' puede ser {string.Join(" o ", active.Select(u => $"{u.Name} ({u.Code})"))}. Escribe su código.");
                return null;
            }

            var unit = active.FirstOrDefault() ?? matches.FirstOrDefault();
            if (unit is null)
            {
                errors.Add($"La unidad de medida '{text.Trim()}' no existe. Elige una de la lista.");
                return null;
            }

            if (UnitOfMeasure.UsableError(unit, text) is { } unitError)
            {
                errors.Add(unitError);
                return null;
            }

            return unit;
        }

        public static IgvAffectation? IgvAffectation(string? text, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                errors.Add("La afectación del IGV es requerida.");
                return null;
            }

            // Igual que la unidad: sin distinguir mayúsculas ni tildes ("Operacion" es "Operación").
            foreach (var igv in Enum.GetValues<Domain.Catalogs.IgvAffectation>())
                if (MatchesIgnoringAccents(text, igv.Description) || MatchesIgnoringAccents(text, igv.ToString()))
                    return igv;

            errors.Add($"La afectación del IGV '{text.Trim()}' no existe. Elige una de la lista.");
            return null;
        }

        public static decimal? Price(ProductSheetRow row, List<string> errors)
        {
            // Con más de 2 decimales la fila queda con error (Product.SalePriceError): no se redondea sin avisar.
            if (row.SalePrice is not null)
                return row.SalePrice.Value;

            if (!string.IsNullOrWhiteSpace(row.SalePriceText) && row.SalePriceText.Contains(','))
                errors.Add($"El precio de venta '{row.SalePriceText.Trim()}' tiene coma. Usa punto para los decimales y no separes los miles con comas, por ejemplo 1500.50.");
            else if (!string.IsNullOrWhiteSpace(row.SalePriceText))
                errors.Add($"El precio de venta '{row.SalePriceText.Trim()}' no es un número.");
            else
                errors.Add("El precio de venta es requerido.");

            return null;
        }

        private static bool MatchesIgnoringAccents(string text, string value) =>
            string.Compare(text.Trim(), value, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
    }
}
