namespace ERP.Application.Features.Audit.ListAuditEntries
{
    /// <summary>Filtros ya resueltos para la consulta: el rango de fechas en UTC, con el final excluido.</summary>
    public sealed record AuditLogFilter(
        int Page,
        int PageSize,
        AuditEntityType? EntityType,
        Guid? EntityId,
        Guid? UserId,
        AuditAction? Action,
        DateTime? FromUtc,
        DateTime? ToUtcExclusive,
        string? SearchTerm
        );
}
