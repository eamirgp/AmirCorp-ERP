using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.CreateBusinessPartner
{
    public sealed record CreateBusinessPartnerDto(
        IdentityDocumentType IdentityDocumentType,
        string DocumentNumber,
        string CountryCode,
        string Name,
        bool IsClient,
        bool IsSupplier
        );
}
