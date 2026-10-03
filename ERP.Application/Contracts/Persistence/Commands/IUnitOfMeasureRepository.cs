using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IUnitOfMeasureRepository
    {
        Task<UnitOfMeasure?> GetByIdAsync(Guid id);
        Task<UnitOfMeasure?> GetByCodeAsync(string code);
        Task<IReadOnlyCollection<UnitOfMeasure>> GetByCodesAsync(IReadOnlyCollection<string> codes);

        /// <summary>Todo el catálogo (son pocas): para leer la planilla de productos.</summary>
        Task<IReadOnlyCollection<UnitOfMeasure>> ListAllAsync();

        /// <summary>Cuántos productos usan la unidad, activos o no.</summary>
        Task<int> CountProductsUsingAsync(string code);

        uint VersionOf(UnitOfMeasure unit);
    }
}
