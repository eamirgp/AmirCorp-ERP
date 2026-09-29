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

            if (Screen is null)
                errors.Add("La pantalla es requerida.");

            SavedViewRequestRules.Validate(Name, Filters, errors);

            return errors;
        }

        public CreateSavedViewDto ToDto() =>
            new(Screen!.Value, Name!, Filters!, IsDefault ?? false);
    }
}
