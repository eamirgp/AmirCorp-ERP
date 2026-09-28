using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Products.GetProduct;
using ERP.Application.Features.Products.ListProducts;
using ERP.Domain.Products;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Queries
{
    internal sealed class ProductQueries : IProductQueries
    {
        private readonly ErpDbContext _context;

        public ProductQueries(ErpDbContext context) => _context = context;

        public async Task<PagedResult<ListProductsResponseDto>> ListProductsAsync(ListProductsDto listProductsDto)
        {
            var query = _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(listProductsDto.SearchTerm))
            {
                var codeTerm = Product.NormalizeCode(listProductsDto.SearchTerm);
                var nameTerm = listProductsDto.SearchTerm.ToLower();

                query = query
                    .Where(p =>
                    p.Code.StartsWith(codeTerm) ||
                    p.Name.ToLower().Contains(nameTerm)
                    );
            }

            if (listProductsDto.IsActive is not null)
                query = query
                    .Where(p => p.IsActive == listProductsDto.IsActive);

            var totalCount = await query.CountAsync();

            query = (listProductsDto.SortBy, listProductsDto.SortDescending) switch
            {
                (ProductSortBy.Name, false) => query.OrderBy(p => p.Name).ThenBy(p => p.Id),
                (ProductSortBy.Name, true) => query.OrderByDescending(p => p.Name).ThenByDescending(p => p.Id),
                (ProductSortBy.CreatedAt, false) => query.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id),
                _ => query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            };

            var items = await query
                .Skip((listProductsDto.Page - 1) * listProductsDto.PageSize)
                .Take(listProductsDto.PageSize)
                .Select(p => new ListProductsResponseDto(
                    p.Id,
                    p.Code,
                    p.Name,
                    p.UnitOfMeasure,
                    p.IgvAffectation,
                    p.SalePrice,
                    p.IsActive
                    ))
                .ToArrayAsync();

            return new PagedResult<ListProductsResponseDto>(
                items,
                listProductsDto.Page,
                listProductsDto.PageSize,
                totalCount
                );
        }

        public async Task<GetProductResponseDto?> GetProductAsync(GetProductDto getProductDto) =>
            await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == getProductDto.Id)
            .Select(p => new GetProductResponseDto(
                p.Id,
                p.Code,
                p.Name,
                p.UnitOfMeasure,
                p.IgvAffectation,
                p.SalePrice,
                p.IsActive,
                p.CreatedAt,
                _context.Users.Where(u => u.Id == p.CreatedBy).Select(u => u.Name).FirstOrDefault(),
                p.UpdatedAt,
                _context.Users.Where(u => u.Id == p.UpdatedBy).Select(u => u.Name).FirstOrDefault()
                ))
            .FirstOrDefaultAsync();
    }
}
