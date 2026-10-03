using ERP.Domain.Partners;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    /// <param name="Reason">Motivo opcional ("Mercadería defectuosa"); queda en la lista y en el historial.</param>
    public sealed record BlockBusinessPartnerRoleRequest(string? Reason)
    {
        public IReadOnlyCollection<string> Validate() =>
            BusinessPartner.BlockReasonError(Reason) is { } error ? [error] : [];
    }
}
