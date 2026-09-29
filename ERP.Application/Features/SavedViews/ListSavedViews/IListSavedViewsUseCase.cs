using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.SavedViews.ListSavedViews
{
    public interface IListSavedViewsUseCase : IQueryUseCase<ListSavedViewsDto, IReadOnlyCollection<ListSavedViewsResponseDto>> { }
}
