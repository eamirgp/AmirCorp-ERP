using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Catalogs.ListIgvAffectations
{
    public interface IListIgvAffectationsUseCase : IQueryUseCase<IReadOnlyCollection<ListIgvAffectationsResponseDto>> { }
}
