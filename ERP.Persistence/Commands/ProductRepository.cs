using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Products;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Commands
{
    internal sealed class ProductRepository : IProductRepository
    {
        private readonly ErpDbContext _context;

        public ProductRepository(ErpDbContext context) => _context = context;

        public void Add(Product product) =>
            _context.Products
            .Add(product);

        public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        {
            var normalizedCode = Product.NormalizeCode(code);

            return await _context.Products
                .Where(p => excludeId == null || p.Id != excludeId)
                .AnyAsync(p => p.Code == normalizedCode);
        }

        public async Task<Product?> GetByIdAsync(Guid id) =>
            await _context.Products
            .Include(p => p.SupplierCodes)
            .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IReadOnlyCollection<SupplierCodeInUse>> SupplierCodesInUseAsync(IReadOnlyCollection<(Guid SupplierId, string Code)> codes, Guid? excludeProductId = null)
        {
            if (codes.Count == 0)
                return [];

            var supplierIds = codes.Select(c => c.SupplierId).Distinct().ToArray();
            var values = codes.Select(c => ProductSupplierCode.NormalizeCode(c.Code)).Distinct().ToArray();

            // Se filtra por proveedores y por códigos, y el par exacto se compara en memoria (son pocos).
            var candidates = await _context.ProductSupplierCodes
                .Where(c => supplierIds.Contains(c.SupplierId) && values.Contains(c.Code))
                .Where(c => excludeProductId == null || c.ProductId != excludeProductId)
                .Join(_context.Products, c => c.ProductId, p => p.Id, (c, p) => new SupplierCodeInUse(c.SupplierId, c.Code, p.Code, p.Name))
                .ToListAsync();

            return candidates
                .Where(c => codes.Any(x => x.SupplierId == c.SupplierId && ProductSupplierCode.NormalizeCode(x.Code) == c.Code))
                .ToList();
        }

        public async Task<IReadOnlyCollection<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids) =>
            await _context.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        public async Task<IReadOnlyCollection<Product>> GetByCodesAsync(IReadOnlyCollection<string> codes)
        {
            var normalizedCodes = codes.Select(Product.NormalizeCode).ToArray();

            return await _context.Products
                .Where(p => normalizedCodes.Contains(p.Code))
                .ToListAsync();
        }

        public uint VersionOf(Product product) =>
            _context.Entry(product).Property<uint>("RowVersion").CurrentValue;
    }
}
