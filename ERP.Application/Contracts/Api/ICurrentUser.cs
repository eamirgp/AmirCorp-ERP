using ERP.Domain.Users.Enums;

namespace ERP.Application.Contracts.Api
{
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        Guid Id { get; }
        UserRole Role { get; }
    }
}
