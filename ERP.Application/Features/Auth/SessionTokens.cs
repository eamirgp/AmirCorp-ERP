using ERP.Application.Contracts.Infrastructure;
using ERP.Domain.Users;

namespace ERP.Application.Features.Auth
{
    /// <summary>Arma los tokens de una sesión: el de acceso (corto) y el refresh token (con su huella para la base).</summary>
    internal sealed class SessionTokens
    {
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;

        public SessionTokens(IJwtService jwtService, IRefreshTokenGenerator refreshTokenGenerator)
        {
            _jwtService = jwtService;
            _refreshTokenGenerator = refreshTokenGenerator;
        }

        /// <summary>Al iniciar sesión: el primer refresh token de la sesión.</summary>
        public (AuthSessionDto Session, RefreshToken Stored) Start(User user, DateTime now)
        {
            var (token, hash) = _refreshTokenGenerator.Generate();
            var stored = RefreshToken.Start(user.Id, hash, now);
            return (new AuthSessionDto(AccessFor(user, stored.FamilyId), token, stored.ExpiresAt), stored);
        }

        /// <summary>Al renovar: anula el token actual y entrega el siguiente de la misma sesión.</summary>
        public (AuthSessionDto Session, RefreshToken Stored) Rotate(RefreshToken current, User user, DateTime now)
        {
            var (token, hash) = _refreshTokenGenerator.Generate();
            var stored = current.Rotate(hash, now);
            return (new AuthSessionDto(AccessFor(user, stored.FamilyId), token, stored.ExpiresAt), stored);
        }

        /// <summary>Solo un token de acceso nuevo, sin tocar la cookie (otra pestaña ya renovó la sesión).</summary>
        public AuthSessionDto AccessOnly(User user, Guid sessionId, DateTime sessionExpiresAt) =>
            new(AccessFor(user, sessionId), null, sessionExpiresAt);

        public string HashOf(string refreshToken) =>
            _refreshTokenGenerator.Hash(refreshToken);

        // Lleva los datos de hoy: si cambió el nombre o el rol, el token nuevo ya los tiene.
        private string AccessFor(User user, Guid sessionId) =>
            _jwtService.GenerateToken(user.Id, user.Name, user.Email, user.Role.ToString(), sessionId);
    }
}
