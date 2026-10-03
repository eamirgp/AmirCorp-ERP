using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Users;

namespace ERP.Application.Features.Auth.Login
{
    internal sealed class LoginUseCase : ILoginUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordService _passwordService;
        private readonly SessionTokens _sessionTokens;
        private readonly ILoginThrottle _loginThrottle;
        private readonly TimeProvider _timeProvider;
        private readonly IUnitOfWork _unitOfWork;

        public LoginUseCase(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordService passwordService,
            SessionTokens sessionTokens,
            ILoginThrottle loginThrottle,
            TimeProvider timeProvider,
            IUnitOfWork unitOfWork
            )
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordService = passwordService;
            _sessionTokens = sessionTokens;
            _loginThrottle = loginThrottle;
            _timeProvider = timeProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthSessionDto>> ExecuteAsync(LoginDto request)
        {
            // Después de varias contraseñas equivocadas seguidas hay que esperar: así no se pueden probar muchas. El
            // intento se cuenta antes de revisar la contraseña, para que varios enviados a la vez no se salten el límite.
            if (_loginThrottle.TryBegin(request.Email, request.ClientIp) is { } wait)
                return Result<AuthSessionDto>.Failure(
                    [$"Demasiados intentos fallidos. Espera {Math.Max(1, (int)Math.Ceiling(wait.TotalSeconds))} segundos e inténtalo de nuevo."],
                    ErrorType.TooManyRequests);

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user is null)
            {
                _passwordService.VerifyDummy(request.Password);
                return Result<AuthSessionDto>.Failure(["Credenciales incorrectas."], ErrorType.Unauthorized);
            }

            if (!_passwordService.Verify(request.Password, user.PasswordHash))
                return Result<AuthSessionDto>.Failure(["Credenciales incorrectas."], ErrorType.Unauthorized);

            _loginThrottle.Succeeded(request.Email, request.ClientIp);

            if (!user.IsActive)
                return Result<AuthSessionDto>.Failure([User.DeactivatedError], ErrorType.Unauthorized);

            // Una sesión nueva en este navegador. De paso se borran las sesiones viejas de este usuario que ya vencieron.
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            await _refreshTokenRepository.RemoveExpiredForUserAsync(user.Id, now);
            var (session, stored) = _sessionTokens.Start(user, now);
            _refreshTokenRepository.Add(stored);

            await _unitOfWork.SaveChangesAsync();

            return Result<AuthSessionDto>.Success(session);
        }
    }
}
