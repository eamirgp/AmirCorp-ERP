using ERP.Domain.Catalogs;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.UpdateBusinessPartner
{
    public sealed record UpdateBusinessPartnerDto(
        Guid Id,
        IdentityDocumentType IdentityDocumentType,
        string DocumentNumber,
        string CountryCode,
        string Name,
        bool IsClient,
        bool IsSupplier,
        uint RowVersion
        );
}
