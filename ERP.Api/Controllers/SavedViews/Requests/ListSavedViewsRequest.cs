using ERP.Application.Features.SavedViews.ListSavedViews;
using ERP.Domain.SavedViews.Enums;

namespace ERP.Api.Controllers.SavedViews.Requests
{
    public sealed record ListSavedViewsRequest(SavedViewScreen? Screen)
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (Screen is null)
                errors.Add("La pantalla es requerida.");

            return errors;
        }

        public ListSavedViewsDto ToDto() =>
            new(Screen!.Value);
    }
}
