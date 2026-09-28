namespace ERP.Application.Contracts.Infrastructure
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string name, string email, string role);
    }
}
