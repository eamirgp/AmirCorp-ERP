namespace ERP.Application.Features.Audit
{
    /// <summary>Un campo modificado, con los valores ya listos para mostrar ("S/ 25.00" → "S/ 30.00").</summary>
    public sealed record AuditChangeDto(string Field, string From, string To);
}
