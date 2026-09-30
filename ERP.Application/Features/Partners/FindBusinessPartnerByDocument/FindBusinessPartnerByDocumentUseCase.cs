using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.FindBusinessPartnerByDocument
{
    /// <summary>
    /// Quién tiene ya ese documento y qué se puede hacer: si está en la otra lista, el formulario ofrece agregarlo
    /// también a esta en vez de crear un duplicado.
    /// </summary>
    public sealed record FoundBusinessPartnerDto(
        Guid Id,
        string Name,
        bool IsClient,
        bool IsSupplier,
        bool IsActive,
        // Si se le puede agregar ese rol: no lo tiene todavía y su documento lo permite.
        bool CanAddClientRole,
        bool CanAddSupplierRole
        )
    {
        public string RoleDescription => BusinessPartnerRules.RoleDescription(IsClient, IsSupplier);
    }

    public interface IFindBusinessPartnerByDocumentUseCase
    {
        Task<FoundBusinessPartnerDto?> ExecuteAsync(IdentityDocumentType identityDocumentType, string documentNumber);
    }

    internal sealed class FindBusinessPartnerByDocumentUseCase : IFindBusinessPartnerByDocumentUseCase
    {
        private readonly IBusinessPartnerRepository _businessPartnerRepository;

        public FindBusinessPartnerByDocumentUseCase(IBusinessPartnerRepository businessPartnerRepository) =>
            _businessPartnerRepository = businessPartnerRepository;

        public async Task<FoundBusinessPartnerDto?> ExecuteAsync(IdentityDocumentType identityDocumentType, string documentNumber)
        {
            if (!Enum.IsDefined(identityDocumentType) || string.IsNullOrWhiteSpace(documentNumber))
                return null;

            if (await _businessPartnerRepository.FindByDocumentAsync(identityDocumentType, documentNumber) is not { } partner)
                return null;

            return new FoundBusinessPartnerDto(
                partner.Id,
                partner.Name,
                partner.IsClient,
                partner.IsSupplier,
                partner.IsActive,
                partner.IsActive && !partner.IsClient && partner.IdentityDocumentType.CanBeClient,
                partner.IsActive && !partner.IsSupplier && partner.IdentityDocumentType.CanIssueTaxDocuments
                );
        }
    }
}
