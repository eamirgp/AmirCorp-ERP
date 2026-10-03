using ERP.Application.Common.Results;

namespace ERP.Application.Features.Partners.AddBusinessPartnerRole
{
    public interface IAddBusinessPartnerRoleUseCase
    {
        Task<Result> ExecuteAsync(Guid id, BusinessPartnerRole role);
    }
}
