using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.UnitsOfMeasure;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Commands
{
    internal sealed class UnitOfMeasureRepository : IUnitOfMeasureRepository
    {
        private readonly ErpDbContext _context;

        public UnitOfMeasureRepository(ErpDbContext context) => _context = context;

        public async Task<UnitOfMeasure?> GetByIdAsync(Guid id) =>
            await _context.UnitsOfMeasure
            .FindAsync(id);

        public async Task<UnitOfMeasure?> GetByCodeAsync(string code)
        {
            var normalized = UnitOfMeasure.NormalizeCode(code);

            return await _context.UnitsOfMeasure
                .FirstOrDefaultAsync(u => u.Code == normalized);
        }

        public async Task<IReadOnlyCollection<UnitOfMeasure>> GetByCodesAsync(IReadOnlyCollection<string> codes)
        {
            var normalized = codes.Select(UnitOfMeasure.NormalizeCode).ToArray();

            return await _context.UnitsOfMeasure
                .Where(u => normalized.Contains(u.Code))
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<UnitOfMeasure>> ListAllAsync() =>
            await _context.UnitsOfMeasure
            .AsNoTracking()
            .ToListAsync();

        public uint VersionOf(UnitOfMeasure unit) =>
            _context.Entry(unit).Property<uint>("RowVersion").CurrentValue;

        public async Task<int> CountProductsUsingAsync(string code) =>
            await _context.Products
            .CountAsync(p => p.UnitOfMeasureCode == code);
    }
}
