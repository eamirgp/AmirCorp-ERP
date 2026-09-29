using ERP.Domain.Catalogs;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    /// <summary>
    /// Validación de los datos de un cliente o proveedor, igual al crear y al editar. Los mensajes del documento
    /// son los mismos del dominio (<see cref="IdentityDocumentTypeExtensions.DocumentNumberError"/>).
    /// </summary>
    internal static class BusinessPartnerRequestRules
    {
        public static List<string> Validate(
            IdentityDocumentType? identityDocumentType,
            string? documentNumber,
            Country? country,
            string? name,
            bool? isClient,
            bool? isSupplier)
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
                if (country is null)
                    errors.Add("Elige el país del proveedor.");
                else if (!Enum.IsDefined(country.Value))
                    errors.Add("El país es inválido.");
                else if (country.Value.IsPeru)
                    errors.Add("Un documento extranjero no puede ser de Perú. Elige el país del proveedor.");
            }

            if (string.IsNullOrWhiteSpace(name))
                errors.Add("El nombre es requerido.");
            else if (BusinessPartner.NormalizeName(name).Length > BusinessPartner.NameMaxLength)
                errors.Add($"El nombre no puede exceder los {BusinessPartner.NameMaxLength} caracteres.");

            if (isClient is null || isSupplier is null)
                errors.Add("Marca si es cliente, proveedor o ambos.");
            else if (isClient is false && isSupplier is false)
                errors.Add("Marca si es cliente, proveedor o ambos.");

            if (isSupplier is true && identityDocumentType is { } t && Enum.IsDefined(t) && !t.CanIssueTaxDocuments)
                errors.Add("Un proveedor debe tener RUC o documento extranjero: con DNI no puede emitir facturas.");

            return errors;
        }

        public static Country CountryFor(IdentityDocumentType identityDocumentType, Country? country) =>
            BusinessPartner.CountryFor(identityDocumentType, country)!.Value;
    }
}
