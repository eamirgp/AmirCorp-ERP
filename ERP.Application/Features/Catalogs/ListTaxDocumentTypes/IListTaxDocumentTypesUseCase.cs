using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Catalogs.ListTaxDocumentTypes
{
    public interface IListTaxDocumentTypesUseCase : IQueryUseCase<IReadOnlyCollection<ListTaxDocumentTypesResponseDto>> { }
}
