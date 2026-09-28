namespace ERP.Application.Contracts.Infrastructure
{
    public interface IPasswordService
    {
        string Hash(string password);
        bool Verify(string password, string passwordHash);
        bool VerifyDummy(string password);
    }
}
