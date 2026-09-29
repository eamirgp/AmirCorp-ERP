using ERP.Domain.Products;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IProductRepository
    {
        void Add(Product product);
        Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
        Task<Product?> GetByIdAsync(Guid id);
        Task<IReadOnlyCollection<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids);
        Task<IReadOnlyCollection<Product>> GetByCodesAsync(IReadOnlyCollection<string> codes);

        /// <summary>Versión actual del producto en la base (cambia con cada modificación).</summary>
        uint VersionOf(Product product);
    }
}
