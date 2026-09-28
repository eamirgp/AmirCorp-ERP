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

        public async Task<bool> DocumentExistsAsync(Guid companyId, TaxDocumentType taxDocumentType, Guid supplierId, string serie, string number)
        {
            var normalizedSerie = Purchase.NormalizeSerie(serie);

            return await _context.Purchases
                .Where(p => !p.IsCancelled)
                .AnyAsync(p =>
                    p.CompanyId == companyId &&
                    p.TaxDocumentType == taxDocumentType &&
                    p.SupplierId == supplierId &&
                    p.Serie == normalizedSerie &&
                    p.Number == number
                    );
        }

        public async Task<Purchase?> GetByIdWithLinesAsync(Guid id) =>
            await _context.Purchases
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
