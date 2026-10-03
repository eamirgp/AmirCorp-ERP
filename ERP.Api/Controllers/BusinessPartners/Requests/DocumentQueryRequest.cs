using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    /// <summary>
    /// Un documento en la dirección de la consulta (?identityDocumentType=Ruc&amp;documentNumber=…). Los dos son
    /// obligatorios: sin el tipo, ASP.NET pondría el primer valor del enum (documento extranjero) y la búsqueda no
    /// encontraría al que sí existe.
    /// </summary>
    public sealed record DocumentQueryRequest(IdentityDocumentType? IdentityDocumentType, string? DocumentNumber)
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (BusinessPartner.DocumentTypeError(IdentityDocumentType) is { } typeError)
                errors.Add(typeError);

            if (string.IsNullOrWhiteSpace(DocumentNumber))
                errors.Add("El número de documento es requerido.");

            return errors;
        }
    }
}
