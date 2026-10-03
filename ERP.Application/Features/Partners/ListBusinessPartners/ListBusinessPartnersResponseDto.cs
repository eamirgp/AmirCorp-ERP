using ERP.Domain.Catalogs;
using ERP.Domain.Partners;
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
        bool IsPurchasingBlocked,
        string? PurchasingBlockReason,
        bool IsSalesBlocked,
        string? SalesBlockReason,
        // Versión del registro: el formulario la devuelve al editar para no pisar cambios de otra persona.
        uint RowVersion
        )
    {
        public string IdentityDocumentTypeDescription => IdentityDocumentType.Description;
        public string CountryName => Countries.NameOf(CountryCode);
        public string RoleDescription => BusinessPartnerRules.RoleDescription(IsClient, IsSupplier);

        /// <summary>Estado en la lista de proveedores: "Activo" o "Compras bloqueadas".</summary>
        public string SupplierStatus => IsPurchasingBlocked ? "Compras bloqueadas" : "Activo";

        /// <summary>Estado en la lista de clientes: "Activo" o "Ventas bloqueadas".</summary>
        public string ClientStatus => IsSalesBlocked ? "Ventas bloqueadas" : "Activo";

        /// <summary>Si se puede "registrar también como cliente": no lo es todavía y su documento lo permite.</summary>
        public bool CanAddClientRole => BusinessPartner.CanAddClientRole(IdentityDocumentType, IsClient);

        /// <summary>Si se puede "registrar también como proveedor": no lo es todavía y su documento lo permite.</summary>
        public bool CanAddSupplierRole => BusinessPartner.CanAddSupplierRole(IdentityDocumentType, IsSupplier);
    }
}
