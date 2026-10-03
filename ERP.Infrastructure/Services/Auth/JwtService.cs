using ERP.Application.Contracts.Infrastructure;
using ERP.Infrastructure.Services.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace ERP.Infrastructure.Services.Auth
{
    internal sealed class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly TimeProvider _timeProvider;

        public JwtService(IOptions<JwtSettings> jwtSettings, TimeProvider timeProvider)
        {
            _jwtSettings = jwtSettings.Value;
            _timeProvider = timeProvider;
        }

        public string GenerateToken(Guid userId, string name, string email, string role, Guid sessionId)
        {
            var handler = new JsonWebTokenHandler();
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Name, name),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role),
                new(IJwtService.SessionClaim, sessionId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                // El mismo reloj que la sesión (TimeProvider), y la emisión explícita: si no, la librería usaría la hora
                // del sistema.
                IssuedAt = now,
                NotBefore = now,
                Expires = now.AddMinutes(_jwtSettings.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)), SecurityAlgorithms.HmacSha256)
            };

            return handler.CreateToken(tokenDescriptor);
        }
    }
}
