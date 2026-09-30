using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.UpdateBusinessPartner
{
    /// <summary>Datos que se corrigen en el formulario. Los roles se agregan con su propia acción.</summary>
    public sealed record UpdateBusinessPartnerDto(
        Guid Id,
        IdentityDocumentType IdentityDocumentType,
        string DocumentNumber,
        string CountryCode,
        string Name,
        uint RowVersion
        );
}
