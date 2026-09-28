using ERP.Application.Common.Pagination;
using ERP.Application.Features.Audit;
using ERP.Application.Features.Audit.ListAuditEntries;

namespace ERP.Api.Controllers.Audit.Requests
{
    public sealed record ListAuditEntriesRequest(
        int? Page,
        int? PageSize,
        AuditEntityType? EntityType,
        Guid? EntityId,
        Guid? UserId,
        AuditAction? Action,
        DateOnly? From,
        DateOnly? To,
        string? SearchTerm
        )
    {
        public ListAuditEntriesDto ToDto() =>
            new(
                PaginationDefaults.NormalizedPage(Page),
                PaginationDefaults.NormalizedPageSize(PageSize),
                EntityType,
                EntityId,
                UserId,
                Action,
                From,
                To,
                SearchTerm
                );
    }
}
