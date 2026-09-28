using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Partners.CreateBusinessPartner
{
    public interface ICreateBusinessPartnerUseCase : IUseCase<CreateBusinessPartnerDto, Result<CreatedResponseDto>> { }
}
