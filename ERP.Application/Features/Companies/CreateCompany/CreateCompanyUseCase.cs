using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Companies;

namespace ERP.Application.Features.Companies.CreateCompany
{
    internal sealed class CreateCompanyUseCase : ICreateCompanyUseCase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCompanyUseCase(
            ICompanyRepository companyRepository,
            IUnitOfWork unitOfWork
            )
        {
            _companyRepository = companyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreatedResponseDto>> ExecuteAsync(CreateCompanyDto request)
        {
            if (await _companyRepository.RucExistsAsync(request.Ruc))
                return Result<CreatedResponseDto>.Failure(["El RUC ya se encuentra en uso."], ErrorType.Conflict);

            var company = Company.Create(
                request.Ruc,
                request.Name
                );

            _companyRepository.Add(company);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(company.Id));
        }
    }
}
