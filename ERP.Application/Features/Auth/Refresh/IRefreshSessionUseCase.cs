using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Auth.Refresh
{
    public interface IRefreshSessionUseCase : IUseCase<string?, Result<AuthSessionDto>> { }
}
