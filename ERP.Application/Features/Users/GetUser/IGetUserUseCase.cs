using ERP.Application.Common.Results;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Users.GetUser
{
    public interface IGetUserUseCase : IQueryUseCase<GetUserDto, Result<GetUserResponseDto>> { }
}
