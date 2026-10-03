using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Partners.UpdateBusinessPartner
{
    internal sealed class UpdateBusinessPartnerUseCase : IUpdateBusinessPartnerUseCase
    {
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBusinessPartnerUseCase(
            IBusinessPartnerRepository businessPartnerRepository,
            IPurchaseRepository purchaseRepository,
            IUnitOfWork unitOfWork
            )
        {
            _businessPartnerRepository = businessPartnerRepository;
            _purchaseRepository = purchaseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(UpdateBusinessPartnerDto request)
        {
            var businessPartner = await _businessPartnerRepository.GetByIdAsync(request.Id);
            if (businessPartner is null)
                return Result.Failure(["El cliente o proveedor no existe."], ErrorType.NotFound);

            // Si alguien lo modificó después de abrir el formulario, no se pisa su cambio.
            if (_businessPartnerRepository.VersionOf(businessPartner) != request.RowVersion)
                return Result.Failure(
                    ["Otra persona modificó este registro mientras lo editabas. Cierra el formulario y vuelve a abrirlo para ver los datos actuales."],
                    ErrorType.Conflict
                    );

            if (await _businessPartnerRepository.FindByDocumentAsync(request.IdentityDocumentType, request.DocumentNumber, request.Id) is { } existing)
                return Result.Failure([BusinessPartnerRules.AlreadyExists(existing)], ErrorType.Conflict);

            var hasPurchases = await _purchaseRepository.ExistsBySupplierAsync(businessPartner.Id);

            // La misma regla del dominio, revisada antes para responder con el mensaje en vez de una excepción.
            if (businessPartner.DocumentChangeError(request.IdentityDocumentType, request.DocumentNumber, hasPurchases) is { } documentError)
                return Result.Failure([documentError], ErrorType.Conflict);

            businessPartner.Update(
                request.IdentityDocumentType,
                request.DocumentNumber,
                request.CountryCode,
                request.Name,
                hasPurchases
                );

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
