using ERP.Application.Features.Partners.UpdateBusinessPartner;
using ERP.Domain.Catalogs;
using DocType = ERP.Domain.Partners.Enums.IdentityDocumentType;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    public sealed record UpdateBusinessPartnerRequest(
        DocType? IdentityDocumentType,
        string? DocumentNumber,
        // Código ISO del país (CN, US…), solo para documento extranjero. Con DNI o RUC se ignora: el país es Perú.
        string? CountryCode,
        string? Name,
        bool? IsClient,
        bool? IsSupplier,
        // Versión que se abrió en el formulario: si otra persona lo cambió mientras tanto, no se pisa su cambio.
        uint? RowVersion
        )
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = BusinessPartnerRequestRules.Validate(IdentityDocumentType, DocumentNumber, CountryCode, Name, IsClient, IsSupplier);

            if (RowVersion is null)
                errors.Add("Falta la versión del registro. Vuelve a abrir el formulario.");

            return errors;
        }

        public UpdateBusinessPartnerDto ToDto(Guid id) =>
            new(
                id,
                IdentityDocumentType!.Value,
                DocumentNumber!,
                BusinessPartnerRequestRules.CountryFor(IdentityDocumentType.Value, CountryCode),
                Name!,
                IsClient!.Value,
                IsSupplier!.Value,
                RowVersion!.Value
                );
    }
}
