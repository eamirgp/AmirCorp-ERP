using System.Security.Cryptography;
using System.Text;
using ERP.Application.Contracts.Infrastructure;

namespace ERP.Infrastructure.Services.Auth
{
    /// <summary>
    /// Refresh tokens de 256 bits aleatorios (imposibles de adivinar). En la base se guarda su SHA-256: alcanza porque el
    /// token ya es aleatorio y largo (no es una contraseña que alguien pueda probar), y permite buscarlo directo.
    /// </summary>
    internal sealed class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public (string Token, string Hash) Generate()
        {
            var token = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            return (token, Hash(token));
        }

        public string Hash(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        private static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
