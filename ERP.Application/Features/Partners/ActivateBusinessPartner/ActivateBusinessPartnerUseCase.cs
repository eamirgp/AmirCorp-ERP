using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Partners.ActivateBusinessPartner
{
    internal sealed class ActivateBusinessPartnerUseCase : IActivateBusinessPartnerUseCase
    {
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateBusinessPartnerUseCase(
            IBusinessPartnerRepository businessPartnerRepository,
            IUnitOfWork unitOfWork
            )
        {
            _businessPartnerRepository = businessPartnerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(ActivateBusinessPartnerDto request)
        {
            var businessPartner = await _businessPartnerRepository.GetByIdAsync(request.Id);
            if (businessPartner is null)
                return Result.Failure(["El cliente o proveedor no existe."], ErrorType.NotFound);

            businessPartner.Activate();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
