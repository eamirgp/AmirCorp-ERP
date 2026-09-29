using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Catalogs.ListUnitsOfMeasure
{
    /// <summary>Solo las unidades activas: son las que la empresa eligió usar en Administración › Unidades de medida.</summary>
    internal sealed class ListUnitsOfMeasureUseCase : IListUnitsOfMeasureUseCase
    {
        private readonly IUnitOfMeasureQueries _unitOfMeasureQueries;

        public ListUnitsOfMeasureUseCase(IUnitOfMeasureQueries unitOfMeasureQueries) => _unitOfMeasureQueries = unitOfMeasureQueries;

        public Task<IReadOnlyCollection<ListUnitsOfMeasureResponseDto>> ExecuteAsync() =>
            _unitOfMeasureQueries.ListActiveAsync();
    }
}
