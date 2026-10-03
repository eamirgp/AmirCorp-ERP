using ERP.Application.Features.Partners.CreateBusinessPartner;
using ERP.Domain.Partners;
using DocType = ERP.Domain.Partners.Enums.IdentityDocumentType;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    public sealed record CreateBusinessPartnerRequest(
        DocType? IdentityDocumentType,
        string? DocumentNumber,
        // Código ISO del país (CN, US…), solo para documento extranjero. Con DNI o RUC se ignora: el país es Perú.
        string? CountryCode,
        string? Name,
        bool? IsClient,
        bool? IsSupplier
        )
    {
        public IReadOnlyCollection<string> Validate() =>
            [
                .. BusinessPartnerRequestRules.Validate(IdentityDocumentType, DocumentNumber, CountryCode, Name),
                .. BusinessPartnerRequestRules.ValidateRoles(IdentityDocumentType, IsClient, IsSupplier),
            ];

        public CreateBusinessPartnerDto ToDto() =>
            new(
                IdentityDocumentType!.Value,
                DocumentNumber!,
                BusinessPartner.CountryFor(IdentityDocumentType.Value, CountryCode)!,
                Name!,
                IsClient!.Value,
                IsSupplier!.Value
                );
    }
}
