using ERP.Application.Common.Results;
using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.SavedViews.DeleteSavedView
{
    /// <summary>Elimina una vista del usuario. Es una preferencia personal: se borra de verdad.</summary>
    internal sealed class DeleteSavedViewUseCase : IDeleteSavedViewUseCase
    {
        private readonly ISavedViewRepository _savedViewRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSavedViewUseCase(
            ISavedViewRepository savedViewRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            )
        {
            _savedViewRepository = savedViewRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(DeleteSavedViewDto request)
        {
            var view = await _savedViewRepository.GetByIdAsync(request.Id);
            if (view is null || view.UserId != _currentUser.Id)
                return Result.Failure(["La vista no existe."], ErrorType.NotFound);

            _savedViewRepository.Remove(view);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
