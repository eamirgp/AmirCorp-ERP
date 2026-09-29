using ERP.Domain.SavedViews.Enums;

namespace ERP.Application.Features.SavedViews.ListSavedViews
{
    public sealed record ListSavedViewsResponseDto(
        Guid Id,
        SavedViewScreen Screen,
        string Name,
        string Filters,
        bool IsDefault
        );
}
