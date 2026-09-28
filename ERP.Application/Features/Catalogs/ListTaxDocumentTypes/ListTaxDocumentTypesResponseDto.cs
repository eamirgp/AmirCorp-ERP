using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListTaxDocumentTypes
{
    public sealed record ListTaxDocumentTypesResponseDto(
        TaxDocumentType TaxDocumentType,
        string Description
        );
}
