using ERP.Application.Common.Pagination;
using ERP.Application.Features.Partners.GetBusinessPartner;
using ERP.Application.Features.Partners.ListBusinessPartners;

namespace ERP.Application.Contracts.Persistence.Queries
{
    public interface IBusinessPartnerQueries
    {
        Task<SortedPagedResult<ListBusinessPartnersResponseDto, BusinessPartnerSortBy>> ListBusinessPartnersAsync(ListBusinessPartnersDto listBusinessPartnersDto);
        Task<GetBusinessPartnerResponseDto?> GetBusinessPartnerAsync(GetBusinessPartnerDto getBusinessPartnerDto);
    }
}
