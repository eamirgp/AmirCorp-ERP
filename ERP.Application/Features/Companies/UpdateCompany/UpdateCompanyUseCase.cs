using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Companies.UpdateCompany
{
    internal sealed class UpdateCompanyUseCase : IUpdateCompanyUseCase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCompanyUseCase(
            ICompanyRepository companyRepository,
            IUnitOfWork unitOfWork
            )
        {
            _companyRepository = companyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(UpdateCompanyDto request)
        {
            var company = await _companyRepository.GetByIdAsync(request.Id);
            if (company is null)
                return Result.Failure(["La empresa no existe."], ErrorType.NotFound);

            if (await _companyRepository.RucExistsAsync(request.Ruc, request.Id))
                return Result.Failure(["El RUC ya se encuentra en uso."], ErrorType.Conflict);

            company.UpdateRuc(request.Ruc);
            company.UpdateName(request.Name);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
