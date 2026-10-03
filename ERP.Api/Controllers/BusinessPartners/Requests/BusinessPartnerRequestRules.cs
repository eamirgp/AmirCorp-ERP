using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    /// <summary>
    /// Revisión previa de un cliente o proveedor, igual al crear y al editar: las mismas reglas del dominio
    /// (<see cref="BusinessPartner.DocumentError"/>, <see cref="BusinessPartner.CountryError"/>…), juntas para avisar
    /// todos los errores de una vez. Los roles solo se piden al crear: después se agregan con su propia acción.
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

            if (BusinessPartner.DocumentError(identityDocumentType, documentNumber) is { } documentError)
                errors.Add(documentError);

            // Con DNI o RUC el país es Perú y no se pregunta; con documento extranjero se elige.
            if (identityDocumentType is { } type && Enum.IsDefined(type)
                && BusinessPartner.CountryError(type, countryCode) is { } countryError)
                errors.Add(countryError);

            if (BusinessPartner.NameError(name) is { } nameError)
                errors.Add(nameError);

            return errors;
        }

        public static string? RowVersionError(uint? rowVersion) =>
            rowVersion is null ? "Falta la versión del registro. Vuelve a abrir la lista." : null;

        /// <summary>Al crear: al menos un rol, y que el documento sirva para ese rol.</summary>
        public static List<string> ValidateRoles(IdentityDocumentType? identityDocumentType, bool? isClient, bool? isSupplier) =>
            BusinessPartner.RolesError(identityDocumentType, isClient ?? false, isSupplier ?? false) is { } error ? [error] : [];
    }
}
