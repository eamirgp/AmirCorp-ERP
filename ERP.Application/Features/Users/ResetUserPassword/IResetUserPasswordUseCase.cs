using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Users.ResetUserPassword
{
    public interface IResetUserPasswordUseCase : IUseCase<ResetUserPasswordDto, Result> { }
}
