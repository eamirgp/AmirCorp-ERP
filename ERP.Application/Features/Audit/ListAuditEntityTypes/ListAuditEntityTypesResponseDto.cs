namespace ERP.Application.Features.Audit.ListAuditEntityTypes
{
    public sealed record ListAuditEntityTypesResponseDto(
        AuditEntityType EntityType,
        string Description
        );
}
