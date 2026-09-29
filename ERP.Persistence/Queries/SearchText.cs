using System.Globalization;
using System.Text;

namespace ERP.Persistence.Queries
{
    /// <summary>
    /// Texto buscado tal como se compara con los nombres: en minúsculas y sin tildes ("Cámara" y "camara" coinciden).
    /// En la base, el nombre pasa por unaccent(), que quita las mismas marcas (también la de la ñ).
    /// </summary>
    internal static class SearchText
    {
        public static string Normalize(string text)
        {
            var decomposed = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var c in decomposed)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
