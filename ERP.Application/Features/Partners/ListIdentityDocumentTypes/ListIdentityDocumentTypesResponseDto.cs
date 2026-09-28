using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.ListIdentityDocumentTypes
{
    public sealed record ListIdentityDocumentTypesResponseDto(
        IdentityDocumentType IdentityDocumentType,
        string Description
        );
}
