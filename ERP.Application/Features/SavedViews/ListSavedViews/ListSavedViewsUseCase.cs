using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.SavedViews.ListSavedViews
{
    /// <summary>Vistas del usuario actual en una pantalla: primero la predeterminada, luego por nombre.</summary>
    internal sealed class ListSavedViewsUseCase : IListSavedViewsUseCase
    {
        private readonly ISavedViewQueries _savedViewQueries;
        private readonly ICurrentUser _currentUser;

        public ListSavedViewsUseCase(ISavedViewQueries savedViewQueries, ICurrentUser currentUser)
        {
            _savedViewQueries = savedViewQueries;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<ListSavedViewsResponseDto>> ExecuteAsync(ListSavedViewsDto request) =>
            await _savedViewQueries.ListAsync(_currentUser.Id, request.Screen);
    }
}
