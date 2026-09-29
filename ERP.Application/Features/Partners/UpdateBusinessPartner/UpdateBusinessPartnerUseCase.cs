using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Partners.UpdateBusinessPartner
{
    internal sealed class UpdateBusinessPartnerUseCase : IUpdateBusinessPartnerUseCase
    {
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBusinessPartnerUseCase(
            IBusinessPartnerRepository businessPartnerRepository,
            IPurchaseRepository purchaseRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork
            )
        {
            _businessPartnerRepository = businessPartnerRepository;
            _purchaseRepository = purchaseRepository;
            _productRepository = productRepository;
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

            var changesDocument = BusinessPartnerRules.DocumentChanges(businessPartner, request.IdentityDocumentType, request.DocumentNumber);
            var dropsSupplierRole = businessPartner.IsSupplier && !request.IsSupplier;

            if (changesDocument || dropsSupplierRole)
            {
                var purchases = await _purchaseRepository.CountBySupplierAsync(businessPartner.Id);

                if (changesDocument && purchases > 0)
                    return Result.Failure([BusinessPartnerRules.DocumentLocked(businessPartner, purchases)], ErrorType.Conflict);

                if (dropsSupplierRole)
                {
                    var productCodes = await _productRepository.CountWithSupplierCodeAsync(businessPartner.Id);
                    if (purchases > 0 || productCodes > 0)
                        return Result.Failure([BusinessPartnerRules.SupplierRoleLocked(businessPartner, purchases, productCodes)], ErrorType.Conflict);
                }
            }

            businessPartner.Update(
                request.IdentityDocumentType,
                request.DocumentNumber,
                request.CountryCode,
                request.Name,
                request.IsClient,
                request.IsSupplier
                );

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
