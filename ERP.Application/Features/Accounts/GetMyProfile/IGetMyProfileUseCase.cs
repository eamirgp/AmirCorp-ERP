using ERP.Application.Common.Results;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Accounts.GetMyProfile
{
    public interface IGetMyProfileUseCase : IQueryUseCase<Result<GetMyProfileResponseDto>> { }
}
