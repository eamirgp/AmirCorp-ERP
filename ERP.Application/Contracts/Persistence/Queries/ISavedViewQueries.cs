using ERP.Application.Features.SavedViews.ListSavedViews;
using ERP.Domain.SavedViews.Enums;

namespace ERP.Application.Contracts.Persistence.Queries
{
    public interface ISavedViewQueries
    {
        Task<IReadOnlyCollection<ListSavedViewsResponseDto>> ListAsync(Guid userId, SavedViewScreen screen);
    }
}
