using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Companies.Activate
{
    internal sealed class ActivateCompanyUseCase : IActivateCompanyUseCase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateCompanyUseCase(
            ICompanyRepository companyRepository,
            IUnitOfWork unitOfWork
            )
        {
            _companyRepository = companyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(ActivateCompanyDto request)
        {
            var company = await _companyRepository.GetByIdAsync(request.Id);
            if (company is null)
                return Result.Failure(["La empresa no existe."], ErrorType.NotFound);

            company.Activate();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
