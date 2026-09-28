using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListTaxDocumentTypes
{
    internal sealed class ListTaxDocumentTypesUseCase : IListTaxDocumentTypesUseCase
    {
        private static readonly IReadOnlyCollection<ListTaxDocumentTypesResponseDto> _taxDocumentTypes =
            Enum.GetValues<TaxDocumentType>()
                .Select(td => new ListTaxDocumentTypesResponseDto(td, td.Description))
                .ToArray();

        public Task<IReadOnlyCollection<ListTaxDocumentTypesResponseDto>> ExecuteAsync() =>
            Task.FromResult(_taxDocumentTypes);
    }
}
