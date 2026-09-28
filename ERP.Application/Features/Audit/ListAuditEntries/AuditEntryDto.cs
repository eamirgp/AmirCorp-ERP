namespace ERP.Application.Features.Audit.ListAuditEntries
{
    /// <summary>Un evento del historial: quién hizo qué, sobre qué registro y cuándo.</summary>
    public sealed record AuditEntryDto(
        Guid Id,
        DateTime OccurredAt,
        Guid UserId,
        string UserName,
        AuditEntityType EntityType,
        Guid EntityId,
        string EntityLabel,
        AuditAction Action,
        IReadOnlyCollection<AuditChangeDto> Changes
        )
    {
        public string EntityTypeDescription => EntityType.Description;
        public string ActionDescription => Action.Description;
    }
}
