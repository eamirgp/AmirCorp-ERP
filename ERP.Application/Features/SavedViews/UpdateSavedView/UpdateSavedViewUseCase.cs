using ERP.Application.Common.Results;
using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.SavedViews;

namespace ERP.Application.Features.SavedViews.UpdateSavedView
{
    /// <summary>Renombra la vista, la actualiza con otros filtros o la marca (o desmarca) como predeterminada.</summary>
    internal sealed class UpdateSavedViewUseCase : IUpdateSavedViewUseCase
    {
        private readonly ISavedViewRepository _savedViewRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSavedViewUseCase(
            ISavedViewRepository savedViewRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            )
        {
            _savedViewRepository = savedViewRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(UpdateSavedViewDto request)
        {
            // Las vistas de otro usuario no existen para quien pregunta.
            var view = await _savedViewRepository.GetByIdAsync(request.Id);
            if (view is null || view.UserId != _currentUser.Id)
                return Result.Failure(["La vista no existe."], ErrorType.NotFound);

            var views = await _savedViewRepository.ListByUserAndScreenAsync(_currentUser.Id, view.Screen);

            if (SavedViewRules.NameTaken(views, request.Name, view.Id))
                return Result.Failure([$"Ya tienes una vista llamada «{SavedView.NormalizeName(request.Name)}»."], ErrorType.Conflict);

            view.Rename(request.Name);
            view.UpdateFilters(request.Filters);

            if (request.IsDefault)
                SavedViewRules.MakeDefault(views, view);
            else
                view.UnmarkAsDefault();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
