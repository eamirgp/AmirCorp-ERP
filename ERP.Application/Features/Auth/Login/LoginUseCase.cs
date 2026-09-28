using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Auth.Login
{
    internal sealed class LoginUseCase : ILoginUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;

        public LoginUseCase(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtService jwtService
            )
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        public async Task<Result<LoginResponseDto>> ExecuteAsync(LoginDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user is null)
            {
                _passwordService.VerifyDummy(request.Password);
                return Result<LoginResponseDto>.Failure(["Credenciales incorrectas."], ErrorType.Unauthorized);
            }

            if (!_passwordService.Verify(request.Password, user.PasswordHash))
                return Result<LoginResponseDto>.Failure(["Credenciales incorrectas."], ErrorType.Unauthorized);

            if (!user.IsActive)
                return Result<LoginResponseDto>.Failure(["La cuenta está desactivada."], ErrorType.Unauthorized);

            return Result<LoginResponseDto>.Success(new LoginResponseDto(_jwtService.GenerateToken(user.Id, user.Name, user.Email, user.Role.ToString())));
        }
    }
}
