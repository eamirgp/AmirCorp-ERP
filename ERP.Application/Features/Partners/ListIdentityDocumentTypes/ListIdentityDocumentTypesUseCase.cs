using ERP.Application.Contracts.Infrastructure;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.ListIdentityDocumentTypes
{
    internal sealed class ListIdentityDocumentTypesUseCase : IListIdentityDocumentTypesUseCase
    {
        private readonly IRucLookup _rucLookup;

        public ListIdentityDocumentTypesUseCase(IRucLookup rucLookup) => _rucLookup = rucLookup;

        public Task<IReadOnlyCollection<ListIdentityDocumentTypesResponseDto>> ExecuteAsync()
        {
            IReadOnlyCollection<ListIdentityDocumentTypesResponseDto> types = Enum.GetValues<IdentityDocumentType>()
                .Select(t => new ListIdentityDocumentTypesResponseDto(t, t.Description, t.LookupSource is not null && _rucLookup.IsConfigured))
                .ToArray();

            return Task.FromResult(types);
        }
    }
}
