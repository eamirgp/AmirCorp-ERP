using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Partners.ListIdentityDocumentTypes
{
    public interface IListIdentityDocumentTypesUseCase : IQueryUseCase<IReadOnlyCollection<ListIdentityDocumentTypesResponseDto>> { }
}
