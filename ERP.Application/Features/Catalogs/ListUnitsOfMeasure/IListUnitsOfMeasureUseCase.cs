using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Catalogs.ListUnitsOfMeasure
{
     public interface IListUnitsOfMeasureUseCase : IQueryUseCase<IReadOnlyCollection<ListUnitsOfMeasureResponseDto>> { }
}
