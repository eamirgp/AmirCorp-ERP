using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Partners.DeactivateBusinessPartner
{
    internal sealed class DeactivateBusinessPartnerUseCase : IDeactivateBusinessPartnerUseCase
    {
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateBusinessPartnerUseCase(
            IBusinessPartnerRepository businessPartnerRepository,
            IUnitOfWork unitOfWork
            )
        {
            _businessPartnerRepository = businessPartnerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(DeactivateBusinessPartnerDto request)
        {
            var businessPartner = await _businessPartnerRepository.GetByIdAsync(request.Id);
            if (businessPartner is null)
                return Result.Failure(["El socio comercial no existe."], ErrorType.NotFound);

            businessPartner.Deactivate();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
