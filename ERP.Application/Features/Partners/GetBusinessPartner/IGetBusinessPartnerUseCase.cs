using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Partners.GetBusinessPartner
{
    public interface IGetBusinessPartnerUseCase : IQueryUseCase<GetBusinessPartnerDto, GetBusinessPartnerResponseDto?> { }
}
