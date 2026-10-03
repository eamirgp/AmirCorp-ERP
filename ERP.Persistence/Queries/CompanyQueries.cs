using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Companies.GetCompany;
using ERP.Application.Features.Companies.ListCompanies;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Queries
{
    internal sealed class CompanyQueries : ICompanyQueries
    {
        private readonly ErpDbContext _context;

        public CompanyQueries(ErpDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<ListCompaniesResponseDto>> ListCompaniesAsync(ListFilterDto filter)
        {
            var query = _context.Companies.AsNoTracking();

            // Por la razón social (sin mayúsculas ni tildes) o por parte del RUC.
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = SearchText.Normalize(filter.SearchTerm);
                query = query.Where(c => EF.Functions.Unaccent(c.Name.ToLower()).Contains(term) || c.Ruc.Contains(term));
            }

            if (filter.IsActive is { } isActive)
                query = query.Where(c => c.IsActive == isActive);

            return await query
            .OrderBy(c => c.Name)
            .Select(c => new ListCompaniesResponseDto(c.Id, c.Ruc, c.Name, c.IsActive, EF.Property<uint>(c, "RowVersion")))
            .ToArrayAsync();
        }

        public async Task<GetCompanyResponseDto?> GetCompanyAsync(GetCompanyDto getCompanyDto) =>
            await _context.Companies
            .AsNoTracking()
            .Where(c => c.Id == getCompanyDto.Id)
            .Select(c => new GetCompanyResponseDto(
                c.Id,
                c.Ruc,
                c.Name,
                c.IsActive,
                c.CreatedAt,
                _context.Users.Where(u => u.Id == c.CreatedBy).Select(u => u.Name).FirstOrDefault(),
                c.UpdatedAt,
                _context.Users.Where(u => u.Id == c.UpdatedBy).Select(u => u.Name).FirstOrDefault()
                ))
            .FirstOrDefaultAsync();
    }
}
