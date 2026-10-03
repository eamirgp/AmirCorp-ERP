using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.EnsureSuperAdmin
{
    internal sealed class EnsureSuperAdminUseCase : IEnsureSuperAdminUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IUnitOfWork _unitOfWork;

        public EnsureSuperAdminUseCase(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IUnitOfWork unitOfWork
            )
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> ExecuteAsync(EnsureSuperAdminDto request)
        {
            if (await _userRepository.RoleExistsAsync(UserRole.SuperAdmin))
                return Result<bool>.Success(false);

            // Las mismas reglas del dominio, juntas: la configuración se corrige de una vez.
            string?[] checks = [User.NameError(request.Name), User.EmailError(request.Email), User.PasswordError(request.Password)];
            var errors = checks.OfType<string>().ToArray();
            if (errors.Length > 0)
                return Result<bool>.Failure(errors, ErrorType.BadRequest);

            if (await _userRepository.EmailExistsAsync(request.Email!))
                return Result<bool>.Failure(["El correo ya se encuentra en uso por otro usuario."], ErrorType.Conflict);

            var user = User.CreateSuperAdmin(request.Name!, request.Email!, request.Password!, _passwordService.Hash);

            _userRepository.Add(user);

            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
