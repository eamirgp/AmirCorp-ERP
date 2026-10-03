using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Catalogs;
using ERP.Domain.Purchases;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Commands
{
    internal sealed class PurchaseRepository : IPurchaseRepository
    {
        private readonly ErpDbContext _context;

        public PurchaseRepository(ErpDbContext context) => _context = context;

        public void Add(Purchase purchase) =>
            _context.Add(purchase);

        public async Task<RegisteredDocument?> FindDocumentAsync(TaxDocumentType taxDocumentType, Guid supplierId, string serie, string number)
        {
            var normalizedSerie = Purchase.NormalizeSerie(serie);

            return await _context.Purchases
                .AsNoTracking()
                .Where(p =>
                    !p.IsCancelled &&
                    p.TaxDocumentType == taxDocumentType &&
                    p.SupplierId == supplierId &&
                    p.Serie == normalizedSerie &&
                    p.Number == number
                    )
                .Select(p => new RegisteredDocument(p.CompanyId, p.CompanyName))
                .FirstOrDefaultAsync();
        }

        public async Task<Purchase?> GetByIdWithLinesAsync(Guid id) =>
            await _context.Purchases
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<bool> ExistsBySupplierAsync(Guid supplierId) =>
            await _context.Purchases
            .AnyAsync(p => p.SupplierId == supplierId);

        public async Task<bool> ExistsByCompanyAsync(Guid companyId) =>
            await _context.Purchases
            .AnyAsync(p => p.CompanyId == companyId);
    }
}
