using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.SavedViews;

namespace ERP.Application.Features.SavedViews.CreateSavedView
{
    internal sealed class CreateSavedViewUseCase : ICreateSavedViewUseCase
    {
        private readonly ISavedViewRepository _savedViewRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateSavedViewUseCase(
            ISavedViewRepository savedViewRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            )
        {
            _savedViewRepository = savedViewRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreatedResponseDto>> ExecuteAsync(CreateSavedViewDto request)
        {
            var views = await _savedViewRepository.ListByUserAndScreenAsync(_currentUser.Id, request.Screen);

            if (views.Count >= SavedView.MaxPerScreen)
                return Result<CreatedResponseDto>.Failure(
                    [$"Ya tienes {SavedView.MaxPerScreen} vistas guardadas en esta pantalla. Elimina alguna para guardar otra."],
                    ErrorType.BadRequest
                    );

            if (SavedViewRules.NameTaken(views, request.Name))
                return Result<CreatedResponseDto>.Failure([$"Ya tienes una vista llamada «{SavedView.NormalizeName(request.Name)}»."], ErrorType.Conflict);

            var view = SavedView.Create(_currentUser.Id, request.Screen, request.Name, request.Filters, isDefault: false);

            if (request.IsDefault)
                SavedViewRules.MakeDefault(views, view);

            _savedViewRepository.Add(view);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(view.Id));
        }
    }
}
