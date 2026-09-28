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
            .FindAsync(id);

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
    }
}
