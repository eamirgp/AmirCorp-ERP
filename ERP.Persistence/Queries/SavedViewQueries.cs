using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.SavedViews.ListSavedViews;
using ERP.Domain.SavedViews.Enums;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Queries
{
    internal sealed class SavedViewQueries : ISavedViewQueries
    {
        private readonly ErpDbContext _context;

        public SavedViewQueries(ErpDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<ListSavedViewsResponseDto>> ListAsync(Guid userId, SavedViewScreen screen) =>
            await _context.SavedViews
            .AsNoTracking()
            .Where(v => v.UserId == userId && v.Screen == screen)
            .OrderByDescending(v => v.IsDefault)
            .ThenBy(v => v.Name)
            .Select(v => new ListSavedViewsResponseDto(
                v.Id,
                v.Screen,
                v.Name,
                v.Filters,
                v.IsDefault
                ))
            .ToArrayAsync();
    }
}
