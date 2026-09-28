using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Partners.UpdateBusinessPartner
{
    internal sealed class UpdateBusinessPartnerUseCase : IUpdateBusinessPartnerUseCase
    {
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBusinessPartnerUseCase(
            IBusinessPartnerRepository businessPartnerRepository,
            IUnitOfWork unitOfWork
            )
        {
            _businessPartnerRepository = businessPartnerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(UpdateBusinessPartnerDto request)
        {
            var businessPartner = await _businessPartnerRepository.GetByIdAsync(request.Id);
            if (businessPartner is null)
                return Result.Failure(["El socio comercial no existe."], ErrorType.NotFound);

            if (await _businessPartnerRepository.DocumentNumberExistsAsync(request.DocumentNumber, request.IdentityDocumentType, request.Id))
                return Result.Failure(["El número de documento ya se encuentra en uso."], ErrorType.Conflict);

            businessPartner.Update(
                request.IdentityDocumentType,
                request.DocumentNumber,
                request.Country,
                request.Name,
                request.IsClient,
                request.IsSupplier
                );

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
