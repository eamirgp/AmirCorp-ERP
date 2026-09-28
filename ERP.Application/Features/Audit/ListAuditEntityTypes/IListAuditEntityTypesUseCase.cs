using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Audit.ListAuditEntityTypes
{
    public interface IListAuditEntityTypesUseCase : IQueryUseCase<IReadOnlyCollection<ListAuditEntityTypesResponseDto>> { }
}
