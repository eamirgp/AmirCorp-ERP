using ERP.Domain.SavedViews;
using ERP.Domain.SavedViews.Enums;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface ISavedViewRepository
    {
        void Add(SavedView savedView);
        void Remove(SavedView savedView);
        Task<SavedView?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<SavedView>> ListByUserAndScreenAsync(Guid userId, SavedViewScreen screen);
    }
}
