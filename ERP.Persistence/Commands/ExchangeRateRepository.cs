using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Catalogs;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Commands
{
    internal sealed class ExchangeRateRepository : IExchangeRateRepository
    {
        private readonly ErpDbContext _context;

        public ExchangeRateRepository(ErpDbContext context) => _context = context;

        public async Task<ExchangeRate?> GetAsync(Currency currency, DateOnly date) =>
            await _context.ExchangeRates
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Currency == currency && e.Date == date);

        public async Task<ExchangeRate?> LatestOnOrBeforeAsync(Currency currency, DateOnly date, DateOnly notBefore) =>
            await _context.ExchangeRates
            .AsNoTracking()
            .Where(e => e.Currency == currency && e.Date <= date && e.Date >= notBefore)
            .OrderByDescending(e => e.Date)
            .FirstOrDefaultAsync();

        public async Task<IReadOnlyCollection<DateOnly>> ExistingDatesAsync(Currency currency, DateOnly from, DateOnly to) =>
            await _context.ExchangeRates
            .Where(e => e.Currency == currency && e.Date >= from && e.Date <= to)
            .Select(e => e.Date)
            .ToListAsync();

        public void Add(ExchangeRate exchangeRate) =>
            _context.ExchangeRates
            .Add(exchangeRate);
    }
}
