using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Auth.Login
{
    public interface ILoginUseCase : IUseCase<LoginDto, Result<LoginResponseDto>> { }
}
