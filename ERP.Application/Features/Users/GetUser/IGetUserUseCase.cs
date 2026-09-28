using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Users.GetUser
{
    public interface IGetUserUseCase : IQueryUseCase<GetUserDto, GetUserResponseDto?> { }
}
