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

            // La regla del dominio (máximo por pantalla, nombre repetido), revisada antes para responder el mensaje.
            if (SavedView.CreateError(request.Name, views) is { } error)
                return Result<CreatedResponseDto>.Failure([error], ErrorType.Conflict);

            var view = SavedView.Create(_currentUser.Id, request.Screen, request.Name, request.Filters, request.IsDefault, views);

            _savedViewRepository.Add(view);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(view.Id));
        }
    }
}
