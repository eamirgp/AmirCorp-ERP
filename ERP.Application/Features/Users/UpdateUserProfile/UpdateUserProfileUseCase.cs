using ERP.Application.Common.Results;
using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Users;

namespace ERP.Application.Features.Users.UpdateUserProfile
{
    internal sealed class UpdateUserProfileUseCase : IUpdateUserProfileUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserProfileUseCase(
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            )
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(UpdateUserProfileDto request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user is null)
                return Result.Failure(["El usuario no existe."], ErrorType.NotFound);

            // Si alguien lo modificó después de abrir el formulario, no se pisa su cambio.
            if (_userRepository.VersionOf(user) != request.RowVersion)
                return Result.Failure([UserRules.ModifiedByOther], ErrorType.Conflict);

            // La misma regla del dominio, revisada antes para responder 403 con el mensaje.
            if (user.ManageError(_currentUser.Role) is { } permissionError)
                return Result.Failure([permissionError], ErrorType.Forbidden);

            if (await _userRepository.FindByEmailAsync(request.Email, request.Id) is { } owner)
                return Result.Failure([User.EmailTakenError(owner)], ErrorType.Conflict);

            user.UpdateProfile(request.Name, request.Email, _currentUser.Role);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
