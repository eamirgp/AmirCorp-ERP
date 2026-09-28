using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Users.UpdateUserProfile
{
    public interface IUpdateUserProfileUseCase : IUseCase<UpdateUserProfileDto, Result> { }
}
