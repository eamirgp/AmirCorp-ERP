using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.FindBusinessPartnerByDocument
{
    internal sealed class FindBusinessPartnerByDocumentUseCase : IFindBusinessPartnerByDocumentUseCase
    {
        private const string Free = "No hay ningún cliente ni proveedor con ese documento.";

        private readonly IBusinessPartnerRepository _businessPartnerRepository;

        public FindBusinessPartnerByDocumentUseCase(IBusinessPartnerRepository businessPartnerRepository) =>
            _businessPartnerRepository = businessPartnerRepository;

        public async Task<Result<FoundBusinessPartnerDto>> ExecuteAsync(IdentityDocumentType identityDocumentType, string documentNumber)
        {
            if (BusinessPartner.DocumentTypeError(identityDocumentType) is { } typeError)
                return Result<FoundBusinessPartnerDto>.Failure([typeError], ErrorType.BadRequest);

            if (string.IsNullOrWhiteSpace(documentNumber)
                || await _businessPartnerRepository.FindByDocumentAsync(identityDocumentType, documentNumber) is not { } partner)
                return Result<FoundBusinessPartnerDto>.Failure([Free], ErrorType.NotFound);

            return Result<FoundBusinessPartnerDto>.Success(new FoundBusinessPartnerDto(
                partner.Id,
                partner.Name,
                partner.IsClient,
                partner.IsSupplier,
                BusinessPartner.CanAddClientRole(partner.IdentityDocumentType, partner.IsClient),
                BusinessPartner.CanAddSupplierRole(partner.IdentityDocumentType, partner.IsSupplier),
                partner.AddClientRoleError(),
                partner.AddSupplierRoleError()
                ));
        }
    }
}
