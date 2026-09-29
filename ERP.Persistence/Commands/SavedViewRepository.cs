using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.SavedViews;
using ERP.Domain.SavedViews.Enums;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Commands
{
    internal sealed class SavedViewRepository : ISavedViewRepository
    {
        private readonly ErpDbContext _context;

        public SavedViewRepository(ErpDbContext context) => _context = context;

        public void Add(SavedView savedView) =>
            _context.SavedViews
            .Add(savedView);

        public void Remove(SavedView savedView) =>
            _context.SavedViews
            .Remove(savedView);

        public async Task<SavedView?> GetByIdAsync(Guid id) =>
            await _context.SavedViews
            .FindAsync(id);

        public async Task<IReadOnlyList<SavedView>> ListByUserAndScreenAsync(Guid userId, SavedViewScreen screen) =>
            await _context.SavedViews
            .Where(v => v.UserId == userId && v.Screen == screen)
            .ToListAsync();
    }
}
