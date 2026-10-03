using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.CreateBusinessPartner
{
    public sealed record CreateBusinessPartnerDto(
        IdentityDocumentType IdentityDocumentType,
        string DocumentNumber,
        // Solo con documento extranjero: con DNI o RUC el dominio pone Perú.
        string? CountryCode,
        string Name,
        bool IsClient,
        bool IsSupplier
        );
}
