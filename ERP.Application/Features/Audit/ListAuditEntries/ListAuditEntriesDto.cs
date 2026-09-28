namespace ERP.Application.Features.Audit.ListAuditEntries
{
    /// <summary>Filtros del historial. Las fechas son días en hora de Perú e incluyen el día final completo.</summary>
    public sealed record ListAuditEntriesDto(
        int Page,
        int PageSize,
        AuditEntityType? EntityType,
        Guid? EntityId,
        Guid? UserId,
        AuditAction? Action,
        DateOnly? From,
        DateOnly? To,
        string? SearchTerm
        );
}
