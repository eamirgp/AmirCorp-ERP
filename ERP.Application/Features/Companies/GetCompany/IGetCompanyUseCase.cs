using ERP.Application.Common.Results;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Companies.GetCompany
{
    public interface IGetCompanyUseCase : IQueryUseCase<GetCompanyDto, Result<GetCompanyResponseDto>> { }
}
