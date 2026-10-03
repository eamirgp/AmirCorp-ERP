using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Users;

namespace ERP.Application.Features.Auth.Login
{
    internal sealed class LoginUseCase : ILoginUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly ILoginThrottle _loginThrottle;

        public LoginUseCase(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtService jwtService,
            ILoginThrottle loginThrottle
            )
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _loginThrottle = loginThrottle;
        }

        public async Task<Result<LoginResponseDto>> ExecuteAsync(LoginDto request)
        {
            // Después de varias contraseñas equivocadas seguidas hay que esperar: así no se pueden probar muchas.
            if (_loginThrottle.WaitFor(request.Email, request.ClientIp) is { } wait)
                return Result<LoginResponseDto>.Failure(
                    [$"Demasiados intentos fallidos. Espera {Math.Max(1, (int)Math.Ceiling(wait.TotalSeconds))} segundos e inténtalo de nuevo."],
                    ErrorType.TooManyRequests);

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user is null)
            {
                _passwordService.VerifyDummy(request.Password);
                _loginThrottle.RecordFailure(request.Email, request.ClientIp);
                return Result<LoginResponseDto>.Failure(["Credenciales incorrectas."], ErrorType.Unauthorized);
            }

            if (!_passwordService.Verify(request.Password, user.PasswordHash))
            {
                _loginThrottle.RecordFailure(request.Email, request.ClientIp);
                return Result<LoginResponseDto>.Failure(["Credenciales incorrectas."], ErrorType.Unauthorized);
            }

            _loginThrottle.Reset(request.Email, request.ClientIp);

            if (!user.IsActive)
                return Result<LoginResponseDto>.Failure([User.DeactivatedError], ErrorType.Unauthorized);

            return Result<LoginResponseDto>.Success(new LoginResponseDto(_jwtService.GenerateToken(user.Id, user.Name, user.Email, user.Role.ToString())));
        }
    }
}
