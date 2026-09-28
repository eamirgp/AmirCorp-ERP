using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.ListIdentityDocumentTypes
{
    internal sealed class ListIdentityDocumentTypesUseCase : IListIdentityDocumentTypesUseCase
    {
        private static readonly IReadOnlyCollection<ListIdentityDocumentTypesResponseDto> _identityDocumentTypes =
            Enum.GetValues<IdentityDocumentType>()
            .Select(idt => new ListIdentityDocumentTypesResponseDto(idt, idt.Description))
            .ToArray();

        public Task<IReadOnlyCollection<ListIdentityDocumentTypesResponseDto>> ExecuteAsync() =>
            Task.FromResult(_identityDocumentTypes);
    }
}
