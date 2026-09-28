using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Companies.Deactivate
{
    public interface IDeactivateCompanyUseCase : IUseCase<DeactivateCompanyDto, Result> { }
}
