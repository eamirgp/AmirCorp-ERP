using ERP.Application.Common.Results;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Partners.GetBusinessPartner
{
    public interface IGetBusinessPartnerUseCase : IQueryUseCase<GetBusinessPartnerDto, Result<GetBusinessPartnerResponseDto>> { }
}
