using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Auth.Logout
{
    /// <summary>Cierra la sesión en el servidor: el refresh token deja de servir, aunque alguien lo hubiera copiado.</summary>
    internal sealed class LogoutUseCase : ILogoutUseCase
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly TimeProvider _timeProvider;
        private readonly IUnitOfWork _unitOfWork;

        public LogoutUseCase(IRefreshTokenRepository refreshTokenRepository, IRefreshTokenGenerator refreshTokenGenerator, TimeProvider timeProvider, IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenGenerator = refreshTokenGenerator;
            _timeProvider = timeProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            if (await _refreshTokenRepository.GetByHashAsync(_refreshTokenGenerator.Hash(refreshToken)) is not { } stored)
                return;

            // Se anula la sesión entera: también los tokens de esta sesión que otra pestaña pudiera tener.
            await _refreshTokenRepository.RevokeFamilyAsync(stored.FamilyId, _timeProvider.GetUtcNow().UtcDateTime);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
