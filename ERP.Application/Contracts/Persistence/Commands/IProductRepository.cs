using ERP.Domain.Products;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IProductRepository
    {
        void Add(Product product);
        /// <summary>El producto que ya tiene ese código interno (sin contar el que se edita), o null.</summary>
        Task<Product?> FindByCodeAsync(string code, Guid? excludeId = null);

        /// <summary>El producto con sus códigos de proveedores, listo para editarse.</summary>
        Task<Product?> GetByIdAsync(Guid id);
        Task<IReadOnlyCollection<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids);
        Task<IReadOnlyCollection<Product>> GetByCodesAsync(IReadOnlyCollection<string> codes);

        /// <summary>Códigos de proveedores que ya usa otro producto (el mismo código del mismo proveedor).</summary>
        Task<IReadOnlyCollection<SupplierCodeInUse>> SupplierCodesInUseAsync(IReadOnlyCollection<(Guid SupplierId, string Code)> codes, Guid? excludeProductId = null);

        /// <summary>Versión actual del producto en la base (cambia con cada modificación).</summary>
        uint VersionOf(Product product);
    }

    public sealed record SupplierCodeInUse(Guid SupplierId, string Code, string ProductCode, string ProductName, bool ProductIsActive);
}
