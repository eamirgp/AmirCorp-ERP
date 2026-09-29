using ERP.Application.Features.Partners.CreateBusinessPartner;
using ERP.Domain.Catalogs;
using DocType = ERP.Domain.Partners.Enums.IdentityDocumentType;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    public sealed record CreateBusinessPartnerRequest(
        DocType? IdentityDocumentType,
        string? DocumentNumber,
        // Solo para documento extranjero. Con DNI o RUC se ignora: el país es Perú.
        Country? Country,
        string? Name,
        bool? IsClient,
        bool? IsSupplier
        )
    {
        public IReadOnlyCollection<string> Validate() =>
            BusinessPartnerRequestRules.Validate(IdentityDocumentType, DocumentNumber, Country, Name, IsClient, IsSupplier);

        public CreateBusinessPartnerDto ToDto() =>
            new(
                IdentityDocumentType!.Value,
                DocumentNumber!,
                BusinessPartnerRequestRules.CountryFor(IdentityDocumentType.Value, Country),
                Name!,
                IsClient!.Value,
                IsSupplier!.Value
                );
    }
}
