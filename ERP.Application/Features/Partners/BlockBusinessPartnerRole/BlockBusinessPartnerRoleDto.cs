using ERP.Application.Features.Partners.AddBusinessPartnerRole;

namespace ERP.Application.Features.Partners.BlockBusinessPartnerRole
{
    /// <param name="Reason">Motivo opcional al bloquear ("Mercadería defectuosa"); se ignora al desbloquear.</param>
    /// <param name="RowVersion">Versión que se veía en la lista.</param>
    public sealed record BlockBusinessPartnerRoleDto(Guid Id, BusinessPartnerRole Role, bool Blocked, string? Reason, uint RowVersion);
}
