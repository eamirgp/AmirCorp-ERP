using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Companies;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Commands
{
    internal sealed class CompanyRepository : ICompanyRepository
    {
        private readonly ErpDbContext _context;

        public CompanyRepository(ErpDbContext context) => _context = context;

        public void Add(Company company) =>
            _context.Companies
            .Add(company);

        public async Task<bool> RucExistsAsync(string ruc, Guid? excludeId = null) =>
            await _context.Companies
            .Where(c => excludeId == null || c.Id != excludeId)
            .AnyAsync(c => c.Ruc == ruc);

        public async Task<Company?> GetByIdAsync(Guid id) =>
            await _context.Companies
            .FindAsync(id);
    }
}
