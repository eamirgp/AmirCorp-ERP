using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Partners.ListBusinessPartners
{
    internal sealed class ListBusinessPartnersUseCase : IListBusinessPartnersUseCase
    {
        private readonly IBusinessPartnerQueries _businessPartnerQueries;

        public ListBusinessPartnersUseCase(IBusinessPartnerQueries businessPartnerQueries) => _businessPartnerQueries = businessPartnerQueries;

        public async Task<PagedResult<ListBusinessPartnersResponseDto>> ExecuteAsync(ListBusinessPartnersDto request) =>
            await _businessPartnerQueries.ListBusinessPartnersAsync(request);
    }
}
