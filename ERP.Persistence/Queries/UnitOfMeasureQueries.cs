using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Catalogs.ListUnitsOfMeasure;
using ERP.Application.Features.UnitsOfMeasure.ListAllUnitsOfMeasure;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Queries
{
    internal sealed class UnitOfMeasureQueries : IUnitOfMeasureQueries
    {
        private readonly ErpDbContext _context;

        public UnitOfMeasureQueries(ErpDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<ListUnitsOfMeasureResponseDto>> ListActiveAsync() =>
            await _context.UnitsOfMeasure
            .AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .Select(u => new ListUnitsOfMeasureResponseDto(u.Code, u.Name, u.FixedConversionFactor))
            .ToArrayAsync();

        public async Task<IReadOnlyCollection<UnitOfMeasureListItemDto>> ListAllAsync(ListFilterDto filter)
        {
            var query = _context.UnitsOfMeasure.AsNoTracking();

            // Por el nombre corto o el de SUNAT (sin mayúsculas ni tildes), o por el código.
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = SearchText.Normalize(filter.SearchTerm);
                query = query.Where(u =>
                    EF.Functions.Unaccent(u.Name.ToLower()).Contains(term)
                    || EF.Functions.Unaccent(u.SunatName.ToLower()).Contains(term)
                    || u.Code.ToLower().Contains(term));
            }

            if (filter.IsActive is { } isActive)
                query = query.Where(u => u.IsActive == isActive);

            return await query
            .OrderByDescending(u => u.IsActive)
            .ThenBy(u => u.Name)
            .Select(u => new UnitOfMeasureListItemDto(
                u.Id,
                u.Code,
                u.Name,
                u.SunatName,
                u.FixedConversionFactor,
                u.IsActive,
                _context.Products.Count(p => p.UnitOfMeasureCode == u.Code),
                EF.Property<uint>(u, "RowVersion")
                ))
            .ToArrayAsync();
        }
    }
}
