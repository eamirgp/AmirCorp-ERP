using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Users.CreateUser
{
    public interface ICreateUserUseCase : IUseCase<CreateUserDto, Result<CreatedResponseDto>> { }
}
