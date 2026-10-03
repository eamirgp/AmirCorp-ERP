using ERP.Application.Features.SavedViews.CreateSavedView;
using ERP.Domain.SavedViews;
using ERP.Domain.SavedViews.Enums;

namespace ERP.Api.Controllers.SavedViews.Requests
{
    public sealed record CreateSavedViewRequest(
        SavedViewScreen? Screen,
        string? Name,
        string? Filters,
        bool? IsDefault
        )
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (SavedView.ScreenError(Screen) is { } screenError)
                errors.Add(screenError);

            SavedViewRequestRules.Validate(Name, Filters, errors);

            return errors;
        }

        public CreateSavedViewDto ToDto() =>
            new(Screen!.Value, Name!, Filters!, IsDefault ?? false);
    }
}
