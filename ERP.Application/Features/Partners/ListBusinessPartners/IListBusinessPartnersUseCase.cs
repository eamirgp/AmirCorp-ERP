using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Pagination;

namespace ERP.Application.Features.Partners.ListBusinessPartners
{
    public interface IListBusinessPartnersUseCase : IQueryUseCase<ListBusinessPartnersDto, PagedResult<ListBusinessPartnersResponseDto>> { }
}
