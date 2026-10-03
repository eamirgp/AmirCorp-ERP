using ERP.Domain.Partners;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    /// <param name="Reason">Motivo opcional ("Mercadería defectuosa"); queda en la lista y en el historial.</param>
    /// <param name="RowVersion">Versión que se veía en la lista: si otra persona lo cambió mientras tanto, no se pisa su cambio.</param>
    public sealed record BlockBusinessPartnerRoleRequest(string? Reason, uint? RowVersion)
    {
        public IReadOnlyCollection<string> Validate() =>
            new[] { BusinessPartner.BlockReasonError(Reason), BusinessPartnerRequestRules.RowVersionError(RowVersion) }
                .OfType<string>()
                .ToArray();
    }

    /// <param name="RowVersion">Versión que se veía en la lista: si otra persona lo cambió mientras tanto, no se pisa su cambio.</param>
    public sealed record UnblockBusinessPartnerRoleRequest(uint? RowVersion)
    {
        public IReadOnlyCollection<string> Validate() =>
            BusinessPartnerRequestRules.RowVersionError(RowVersion) is { } error ? [error] : [];
    }
}
