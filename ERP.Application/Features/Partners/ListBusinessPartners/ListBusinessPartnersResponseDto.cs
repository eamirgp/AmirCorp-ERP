using ERP.Domain.Catalogs;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.ListBusinessPartners
{
    public sealed record ListBusinessPartnersResponseDto(
        Guid Id,
        IdentityDocumentType IdentityDocumentType,
        string DocumentNumber,
        string CountryCode,
        string Name,
        bool IsClient,
        bool IsSupplier,
        bool IsActive,
        // Versión del registro: el formulario la devuelve al editar para no pisar cambios de otra persona.
        uint RowVersion
        )
    {
        public string IdentityDocumentTypeDescription => IdentityDocumentType.Description;
        public string CountryName => Countries.NameOf(CountryCode);
        public string RoleDescription => BusinessPartnerRules.RoleDescription(IsClient, IsSupplier);

        /// <summary>Si se puede "registrar también como cliente": activo, no lo es todavía y su documento lo permite.</summary>
        public bool CanAddClientRole => IsActive && !IsClient && IdentityDocumentType.CanBeClient;

        /// <summary>Si se puede "registrar también como proveedor": activo, no lo es todavía y su documento lo permite.</summary>
        public bool CanAddSupplierRole => IsActive && !IsSupplier && IdentityDocumentType.CanIssueTaxDocuments;
    }
}
