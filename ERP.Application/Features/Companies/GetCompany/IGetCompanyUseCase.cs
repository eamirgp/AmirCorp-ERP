using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Companies.GetCompany
{
    public interface IGetCompanyUseCase : IQueryUseCase<GetCompanyDto, GetCompanyResponseDto?> { }
}
