using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Pagination;

namespace ERP.Application.Features.UnitsOfMeasure.ListAllUnitsOfMeasure
{
    public interface IListAllUnitsOfMeasureUseCase : IQueryUseCase<ListFilterDto, IReadOnlyCollection<UnitOfMeasureListItemDto>> { }
}
