using ERP.Domain.Catalogs;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    /// <summary>
    /// Validación de los datos de un cliente o proveedor, igual al crear y al editar. Los mensajes del documento
    /// son los mismos del dominio (<see cref="IdentityDocumentTypeExtensions.DocumentNumberError"/>).
    /// Los roles solo se piden al crear (<see cref="ValidateRoles"/>): después se agregan con su propia acción.
    /// </summary>
    internal static class BusinessPartnerRequestRules
    {
        public static List<string> Validate(
            IdentityDocumentType? identityDocumentType,
            string? documentNumber,
            string? countryCode,
            string? name)
        {
            var errors = new List<string>();

            if (identityDocumentType is null)
                errors.Add("El tipo de documento es requerido.");
            else if (!Enum.IsDefined(identityDocumentType.Value))
                errors.Add("El tipo de documento es inválido.");
            else if (identityDocumentType.Value.DocumentNumberError(IdentityDocumentTypeExtensions.NormalizeDocumentNumber(documentNumber ?? "")) is { } documentError)
                errors.Add(documentError);

            // Con DNI o RUC el país es Perú y no se pregunta; con documento extranjero se elige.
            if (identityDocumentType is { } type && Enum.IsDefined(type) && !type.RequiresPeruvianCountry)
            {
                if (string.IsNullOrWhiteSpace(countryCode))
                    errors.Add("Elige el país del proveedor.");
                else if (!Countries.Exists(countryCode))
                    errors.Add("El país es inválido.");
                else if (Countries.IsPeru(countryCode))
                    errors.Add("Un documento extranjero no puede ser de Perú. Elige el país del proveedor.");
            }

            if (string.IsNullOrWhiteSpace(name))
                errors.Add("El nombre es requerido.");
            else if (BusinessPartner.NormalizeName(name).Length > BusinessPartner.NameMaxLength)
                errors.Add($"El nombre no puede exceder los {BusinessPartner.NameMaxLength} caracteres.");

            return errors;
        }

        /// <summary>Al crear: al menos un rol, y que el documento sirva para ese rol.</summary>
        public static List<string> ValidateRoles(IdentityDocumentType? identityDocumentType, bool? isClient, bool? isSupplier)
        {
            var errors = new List<string>();

            if (isClient is null || isSupplier is null)
                errors.Add("Marca si es cliente, proveedor o ambos.");
            else if (isClient is false && isSupplier is false)
                errors.Add("Marca si es cliente, proveedor o ambos.");

            if (isSupplier is true && identityDocumentType is { } t && Enum.IsDefined(t) && !t.CanIssueTaxDocuments)
                errors.Add("Un proveedor debe tener RUC o documento extranjero: con DNI no puede emitir facturas.");

            if (isClient is true && identityDocumentType is { } c && Enum.IsDefined(c) && !c.CanBeClient)
                errors.Add("Por ahora solo se vende en Perú: un cliente debe tener RUC o DNI.");

            return errors;
        }

        public static string CountryFor(IdentityDocumentType identityDocumentType, string? countryCode) =>
            BusinessPartner.CountryFor(identityDocumentType, countryCode)!;
    }
}
