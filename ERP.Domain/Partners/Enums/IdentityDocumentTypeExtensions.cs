namespace ERP.Domain.Partners.Enums
{
    public static class IdentityDocumentTypeExtensions
    {
        public const int DniLength = 8;
        public const int RucLength = 11;
        public const int ForeignMaxLength = 20;

        // Prefijos del RUC: 10 persona natural, 15 y 17 no domiciliados y otros, 16 sociedades conyugales, 20 persona jurídica.
        private static readonly string[] RucPrefixes = ["10", "15", "16", "17", "20"];
        private static readonly int[] RucWeights = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];

        /// <summary>El número tal como se guarda: sin espacios y en mayúsculas.</summary>
        public static string NormalizeDocumentNumber(string documentNumber) =>
            string.Concat(documentNumber.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();

        extension(IdentityDocumentType identityDocumentType)
        {
            public string Description => identityDocumentType switch
            {
                IdentityDocumentType.TributarioExtranjero => "Documento Tributario Extranjero",
                IdentityDocumentType.Dni => "DNI",
                IdentityDocumentType.Ruc => "RUC",
                _ => identityDocumentType.ToString()
            };

            /// <summary>DNI y RUC son siempre de Perú: el país no se pregunta.</summary>
            public bool RequiresPeruvianCountry =>
                identityDocumentType is IdentityDocumentType.Dni or IdentityDocumentType.Ruc;

            public bool CanIssueTaxDocuments =>
                identityDocumentType is IdentityDocumentType.Ruc or IdentityDocumentType.TributarioExtranjero;

            public bool IsDomesticTaxpayer =>
                identityDocumentType is IdentityDocumentType.Ruc;

            public bool IsValidDocumentNumber(string documentNumber) =>
                identityDocumentType.DocumentNumberError(documentNumber) is null;

            /// <summary>
            /// Qué tiene de malo el número (ya normalizado), o null si es válido. El RUC se valida como lo hace SUNAT:
            /// 11 dígitos, un prefijo válido y el dígito verificador (módulo 11).
            /// </summary>
            public string? DocumentNumberError(string documentNumber) => identityDocumentType switch
            {
                _ when documentNumber.Length == 0 => "El número de documento es requerido.",

                IdentityDocumentType.Dni when documentNumber.Length != DniLength || !documentNumber.All(char.IsAsciiDigit) =>
                    $"El DNI debe tener {DniLength} dígitos.",

                IdentityDocumentType.Ruc when documentNumber.Length != RucLength || !documentNumber.All(char.IsAsciiDigit) =>
                    $"El RUC debe tener {RucLength} dígitos.",
                IdentityDocumentType.Ruc when !RucPrefixes.Contains(documentNumber[..2]) =>
                    "El RUC debe empezar con 10, 15, 16, 17 o 20.",
                IdentityDocumentType.Ruc when !HasValidRucCheckDigit(documentNumber) =>
                    $"El RUC {documentNumber} no es válido: el último dígito no corresponde. Revisa que esté bien escrito.",

                IdentityDocumentType.TributarioExtranjero when documentNumber.Length > ForeignMaxLength || !documentNumber.All(c => char.IsAsciiLetterOrDigit(c) || c == '-') =>
                    $"El documento extranjero solo puede tener letras, números y guiones, hasta {ForeignMaxLength} caracteres.",

                IdentityDocumentType.Dni or IdentityDocumentType.Ruc or IdentityDocumentType.TributarioExtranjero => null,
                _ => "El tipo de documento es inválido."
            };
        }

        private static bool HasValidRucCheckDigit(string ruc)
        {
            var sum = 0;
            for (var i = 0; i < RucWeights.Length; i++)
                sum += (ruc[i] - '0') * RucWeights[i];

            var check = 11 - sum % 11;
            var expected = check switch { 10 => 0, 11 => 1, _ => check };
            return ruc[10] - '0' == expected;
        }
    }
}
