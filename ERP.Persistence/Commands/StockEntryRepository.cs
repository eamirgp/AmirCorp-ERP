using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Inventory;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Commands
{
    internal sealed class StockEntryRepository : IStockEntryRepository
    {
        private readonly ErpDbContext _context;

        public StockEntryRepository(ErpDbContext context) => _context = context;

        public void AddRange(IReadOnlyCollection<StockEntry> stockEntries) =>
            _context.StockEntries
            .AddRange(stockEntries);

        public async Task<IReadOnlyCollection<StockEntry>> GetByPurchaseLineIdsAsync(IReadOnlyCollection<Guid> purchaseLineIds) =>
            await _context.StockEntries
            .Where(se => purchaseLineIds.Contains(se.PurchaseLineId))
            .ToListAsync();

        public void RemoveRange(IReadOnlyCollection<StockEntry> stockEntries) =>
            _context.StockEntries
            .RemoveRange(stockEntries);
    }
}
