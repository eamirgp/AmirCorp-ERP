using ERP.Application.Features.SavedViews.ListSavedViews;
using ERP.Domain.SavedViews;
using ERP.Domain.SavedViews.Enums;

namespace ERP.Api.Controllers.SavedViews.Requests
{
    public sealed record ListSavedViewsRequest(SavedViewScreen? Screen)
    {
        public IReadOnlyCollection<string> Validate() =>
            SavedView.ScreenError(Screen) is { } error ? [error] : [];

        public ListSavedViewsDto ToDto() =>
            new(Screen!.Value);
    }
}
