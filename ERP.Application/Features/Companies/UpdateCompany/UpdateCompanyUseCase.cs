using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Companies.UpdateCompany
{
    internal sealed class UpdateCompanyUseCase : IUpdateCompanyUseCase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCompanyUseCase(
            ICompanyRepository companyRepository,
            IPurchaseRepository purchaseRepository,
            IUnitOfWork unitOfWork
            )
        {
            _companyRepository = companyRepository;
            _purchaseRepository = purchaseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(UpdateCompanyDto request)
        {
            var company = await _companyRepository.GetByIdAsync(request.Id);
            if (company is null)
                return Result.Failure(["La empresa no existe."], ErrorType.NotFound);

            var hasPurchases = await _purchaseRepository.ExistsByCompanyAsync(company.Id);

            // La misma regla del dominio, revisada antes para responder con el mensaje en vez de una excepción.
            if (company.RucChangeError(request.Ruc, hasPurchases) is { } rucError)
                return Result.Failure([rucError], ErrorType.Conflict);

            if (await _companyRepository.RucExistsAsync(request.Ruc, request.Id))
                return Result.Failure(["El RUC ya se encuentra en uso."], ErrorType.Conflict);

            company.UpdateRuc(request.Ruc, hasPurchases);
            company.UpdateName(request.Name);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
