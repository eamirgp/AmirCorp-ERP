namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    /// <param name="Reason">Motivo opcional ("Mercadería defectuosa"); queda en la lista y en el historial.</param>
    public sealed record BlockBusinessPartnerRoleRequest(string? Reason);
}
