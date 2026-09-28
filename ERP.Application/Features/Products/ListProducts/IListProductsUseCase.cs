using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Pagination;

namespace ERP.Application.Features.Products.ListProducts
{
    public interface IListProductsUseCase : IQueryUseCase<ListProductsDto, PagedResult<ListProductsResponseDto>> { }
}
