using System.Text.RegularExpressions;

namespace ERP.Domain.Common
{
    public static class TextNormalizer
    {
        private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);

        /// <summary>El texto tal como se guarda: sin espacios al inicio ni al final, ni dobles en medio.</summary>
        public static string CollapseSpaces(string text) =>
            Spaces.Replace(text.Trim(), " ");
    }
}
