namespace ERP.Application.Features.SavedViews.UpdateSavedView
{
    public sealed record UpdateSavedViewDto(
        Guid Id,
        string Name,
        string Filters,
        bool IsDefault
        );
}
