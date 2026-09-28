using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Companies.CreateCompany
{
    public interface ICreateCompanyUseCase : IUseCase<CreateCompanyDto, Result<CreatedResponseDto>> { }
}
