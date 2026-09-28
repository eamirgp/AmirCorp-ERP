using ERP.Application.Contracts.Infrastructure;

namespace ERP.Infrastructure.Services.Auth
{
    internal sealed class PasswordService : IPasswordService
    {
        private static readonly string _passwordDummyHash = BCrypt.Net.BCrypt.HashPassword("$DummyDummy123+");

        public string Hash(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool Verify(string password, string passwordHash) =>
            BCrypt.Net.BCrypt.Verify(password, passwordHash);

        public bool VerifyDummy(string password) =>
            BCrypt.Net.BCrypt.Verify(password, _passwordDummyHash);
    }
}
