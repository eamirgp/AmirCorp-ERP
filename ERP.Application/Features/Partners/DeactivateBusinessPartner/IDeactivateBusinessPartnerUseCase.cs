using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Partners.DeactivateBusinessPartner
{
    public interface IDeactivateBusinessPartnerUseCase : IUseCase<DeactivateBusinessPartnerDto, Result> { }
}
