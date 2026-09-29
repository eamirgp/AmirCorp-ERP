using ERP.Domain.SavedViews.Enums;

namespace ERP.Application.Features.SavedViews.CreateSavedView
{
    public sealed record CreateSavedViewDto(
        SavedViewScreen Screen,
        string Name,
        string Filters,
        bool IsDefault
        );
}
