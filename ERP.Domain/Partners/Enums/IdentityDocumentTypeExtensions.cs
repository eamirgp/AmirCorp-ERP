namespace ERP.Domain.Partners.Enums
{
    public static class IdentityDocumentTypeExtensions
    {
        public const int DniLength = 8;
        public const int RucLength = 11;
        public const int ForeignMaxLength = 20;

        extension(IdentityDocumentType identityDocumentType)
        {
            public string Description => identityDocumentType switch
            {
                IdentityDocumentType.TributarioExtranjero => "Documento Tributario Extranjero",
                IdentityDocumentType.Dni => "DNI",
                IdentityDocumentType.Ruc => "RUC",
                _ => identityDocumentType.ToString()
            };

            public bool RequiresPeruvianCountry =>
                identityDocumentType is IdentityDocumentType.Dni or IdentityDocumentType.Ruc;

            public bool CanIssueTaxDocuments =>
                identityDocumentType is IdentityDocumentType.Ruc or IdentityDocumentType.TributarioExtranjero;

            public bool IsDomesticTaxpayer =>
                identityDocumentType is IdentityDocumentType.Ruc;

            public bool IsValidDocumentNumber(string documentNumber) => identityDocumentType switch
            {
                IdentityDocumentType.Dni => documentNumber.Length == DniLength && documentNumber.All(char.IsDigit),
                IdentityDocumentType.Ruc => documentNumber.Length == RucLength && documentNumber.All(char.IsDigit),
                IdentityDocumentType.TributarioExtranjero => documentNumber.Length <= ForeignMaxLength,
                _ => false
            };
        }
    }
}
