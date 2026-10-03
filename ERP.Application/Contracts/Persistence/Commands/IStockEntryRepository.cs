using ERP.Domain.Inventory;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IStockEntryRepository
    {
        void AddRange(IReadOnlyCollection<StockEntry> stockEntries);
        Task<IReadOnlyCollection<StockEntry>> GetByPurchaseLineIdsAsync(IReadOnlyCollection<Guid> purchaseLineIds);
        void RemoveRange(IReadOnlyCollection<StockEntry> stockEntries);
    }
}
