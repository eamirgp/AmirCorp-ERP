using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.UnitsOfMeasure.ListAllUnitsOfMeasure
{
    public interface IListAllUnitsOfMeasureUseCase
    {
        Task<IReadOnlyCollection<UnitOfMeasureListItemDto>> ExecuteAsync();
    }

    /// <summary>Todo el catálogo: primero las activas y luego las demás, cada grupo por nombre de la A a la Z.</summary>
    internal sealed class ListAllUnitsOfMeasureUseCase : IListAllUnitsOfMeasureUseCase
    {
        private readonly IUnitOfMeasureQueries _unitOfMeasureQueries;

        public ListAllUnitsOfMeasureUseCase(IUnitOfMeasureQueries unitOfMeasureQueries) => _unitOfMeasureQueries = unitOfMeasureQueries;

        public Task<IReadOnlyCollection<UnitOfMeasureListItemDto>> ExecuteAsync() =>
            _unitOfMeasureQueries.ListAllAsync();
    }
}
