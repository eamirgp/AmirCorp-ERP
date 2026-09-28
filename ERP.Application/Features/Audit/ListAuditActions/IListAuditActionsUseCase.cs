using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Audit.ListAuditActions
{
    public interface IListAuditActionsUseCase : IQueryUseCase<IReadOnlyCollection<ListAuditActionsResponseDto>> { }
}
