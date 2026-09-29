using ERP.Application.Features.SavedViews.UpdateSavedView;

namespace ERP.Api.Controllers.SavedViews.Requests
{
    public sealed record UpdateSavedViewRequest(
        string? Name,
        string? Filters,
        bool? IsDefault
        )
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();
            SavedViewRequestRules.Validate(Name, Filters, errors);
            return errors;
        }

        public UpdateSavedViewDto ToDto(Guid id) =>
            new(id, Name!, Filters!, IsDefault ?? false);
    }
}
