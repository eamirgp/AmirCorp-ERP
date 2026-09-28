namespace ERP.Application.Features.Audit.ListAuditActions
{
    public sealed record ListAuditActionsResponseDto(
        AuditAction Action,
        string Description
        );
}
