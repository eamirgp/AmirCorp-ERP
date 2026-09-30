using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.ListBusinessPartners
{
    public sealed record ListBusinessPartnersDto(
        int Page,
        int PageSize,
        string? SearchTerm,
        // Bloqueado en el rol que se lista (compras en proveedores, ventas en clientes).
        bool? IsBlocked,
        PartnerRoleFilter? PartnerRoleFilter,
        IdentityDocumentType? IdentityDocumentType,
        BusinessPartnerSortBy SortBy,
        bool SortDescending
        )
    {
        /// <summary>Orden de la lista cuando la pantalla no pide uno: por nombre, de la A a la Z.</summary>
        public const BusinessPartnerSortBy DefaultSortBy = BusinessPartnerSortBy.Name;
        public const bool DefaultSortDescending = false;
    }
}
