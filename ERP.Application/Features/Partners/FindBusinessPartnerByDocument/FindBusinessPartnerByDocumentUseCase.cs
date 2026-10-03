using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.FindBusinessPartnerByDocument
{
    /// <summary>
    /// Quién tiene ya ese documento y qué se puede hacer: si está en la otra lista, el formulario ofrece agregarlo
    /// también a esta en vez de crear un duplicado. Los textos vienen listos (la pantalla no arma frases).
    /// </summary>
    public sealed record FoundBusinessPartnerDto(
        Guid Id,
        string Name,
        bool IsClient,
        bool IsSupplier,
        // Si se le puede agregar ese rol: no lo tiene todavía y su documento lo permite. Un bloqueo en el otro rol
        // no impide agregarlo: cada rol se bloquea por separado.
        bool CanAddClientRole,
        bool CanAddSupplierRole,
        // Por qué no se le puede agregar ese rol ("X ya es cliente.", "Por ahora solo se vende en Perú…"), o null si se puede.
        string? AddClientRoleError,
        string? AddSupplierRoleError
        )
    {
        public string RoleDescription => BusinessPartnerRules.RoleDescription(IsClient, IsSupplier);

        /// <summary>"ACME S.A.C. ya está registrado como proveedor."</summary>
        public string Summary => $"{Name} ya está registrado como {RoleDescription.ToLowerInvariant()}.";
    }

    public interface IFindBusinessPartnerByDocumentUseCase
    {
        /// <returns>Quién tiene ese documento, o 404 si nadie (el documento está libre).</returns>
        Task<Result<FoundBusinessPartnerDto>> ExecuteAsync(IdentityDocumentType identityDocumentType, string documentNumber);
    }

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
