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

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add("El nombre es requerido.");

            if (string.IsNullOrWhiteSpace(request.Email))
                errors.Add("El correo es requerido.");

            if (string.IsNullOrWhiteSpace(request.Password))
                errors.Add("La contraseña es requerida.");
            else if (request.Password.Length < User.PasswordMinLength)
                errors.Add($"La contraseña debe tener al menos {User.PasswordMinLength} caracteres.");

            if (errors.Count > 0)
                return Result<bool>.Failure(errors, ErrorType.BadRequest);

            if (await _userRepository.EmailExistsAsync(request.Email!))
                return Result<bool>.Failure(["El correo ya se encuentra en uso por otro usuario."], ErrorType.Conflict);

            var user = User.Create(
                request.Name!,
                request.Email!,
                _passwordService.Hash(request.Password!),
                UserRole.SuperAdmin
                );

            _userRepository.Add(user);

            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
