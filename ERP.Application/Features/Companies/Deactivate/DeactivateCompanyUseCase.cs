using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Companies.Deactivate
{
    internal sealed class DeactivateCompanyUseCase : IDeactivateCompanyUseCase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateCompanyUseCase(
            ICompanyRepository companyRepository,
            IUnitOfWork unitOfWork
            )
        {
            _companyRepository = companyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(DeactivateCompanyDto request)
        {
            var company = await _companyRepository.GetByIdAsync(request.Id);
            if (company is null)
                return Result.Failure(["La empresa no existe."], ErrorType.NotFound);

            company.Deactivate();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
