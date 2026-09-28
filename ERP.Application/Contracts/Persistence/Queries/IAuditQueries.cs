using ERP.Application.Common.Pagination;
using ERP.Application.Features.Audit.ListAuditEntries;

namespace ERP.Application.Contracts.Persistence.Queries
{
    public interface IAuditQueries
    {
        Task<PagedResult<AuditEntryDto>> ListAsync(AuditLogFilter filter);
    }
}
