using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Pagination;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Audit.ListAuditEntries
{
    public interface IListAuditEntriesUseCase : IQueryUseCase<ListAuditEntriesDto, Result<PagedResult<AuditEntryDto>>> { }
}
