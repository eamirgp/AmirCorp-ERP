using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Users.DeactivateUser
{
    public interface IDeactivateUserUseCase : IUseCase<DeactivateUserDto, Result> { }
}
