using ERP.Application.Features.Catalogs.ListUnitsOfMeasure;
using ERP.Application.Features.UnitsOfMeasure.ListAllUnitsOfMeasure;

namespace ERP.Application.Contracts.Persistence.Queries
{
    public interface IUnitOfMeasureQueries
    {
        /// <summary>Las unidades activas, para elegir en productos, compras y la planilla.</summary>
        Task<IReadOnlyCollection<ListUnitsOfMeasureResponseDto>> ListActiveAsync();

        /// <summary>Todo el catálogo con cuántos productos usan cada unidad, para la pantalla de administración.</summary>
        Task<IReadOnlyCollection<UnitOfMeasureListItemDto>> ListAllAsync();
    }
}
