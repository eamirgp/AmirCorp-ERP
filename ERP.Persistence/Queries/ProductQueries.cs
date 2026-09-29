using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Products.ExportProducts;
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

        public async Task<SortedPagedResult<ListProductsResponseDto, ProductSortBy>> ListProductsAsync(ListProductsDto listProductsDto)
        {
            var query = Filter(_context.Products.AsNoTracking(), listProductsDto.SearchTerm, listProductsDto.IsActive);

            var totalCount = await query.CountAsync();

            // Si la página pedida ya no existe, se devuelve la última.
            var page = PaginationDefaults.ClampPage(listProductsDto.Page, listProductsDto.PageSize, totalCount);

            var items = await Sort(query, listProductsDto.SortBy, listProductsDto.SortDescending)
                .Skip((page - 1) * listProductsDto.PageSize)
                .Take(listProductsDto.PageSize)
                .Select(p => new ListProductsResponseDto(
                    p.Id,
                    p.Code,
                    p.Name,
                    p.UnitOfMeasure,
                    p.IgvAffectation,
                    p.SalePrice,
                    p.IsActive,
                    EF.Property<uint>(p, "RowVersion")
                    ))
                .ToArrayAsync();

            return new SortedPagedResult<ListProductsResponseDto, ProductSortBy>(
                items,
                page,
                listProductsDto.PageSize,
                totalCount,
                listProductsDto.SortBy,
                listProductsDto.SortDescending
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

        public async Task<IReadOnlyCollection<ProductExportRowDto>> ListForExportAsync(ExportProductsDto exportProductsDto) =>
            await Sort(Filter(_context.Products.AsNoTracking(), exportProductsDto.SearchTerm, exportProductsDto.IsActive), exportProductsDto.SortBy, exportProductsDto.SortDescending)
            .Select(p => new ProductExportRowDto(p.Code, p.Name, p.UnitOfMeasure, p.IgvAffectation, p.SalePrice))
            .ToArrayAsync();

        // La lista y la exportación filtran y ordenan igual: el Excel trae exactamente lo que se ve en la pantalla.
        private static IQueryable<Product> Filter(IQueryable<Product> query, string? searchTerm, bool? isActive)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var codeTerm = Product.NormalizeCode(searchTerm);
                var nameTerm = searchTerm.ToLower();

                query = query
                    .Where(p =>
                    p.Code.StartsWith(codeTerm) ||
                    p.Name.ToLower().Contains(nameTerm)
                    );
            }

            if (isActive is not null)
                query = query
                    .Where(p => p.IsActive == isActive);

            return query;
        }

        private static IQueryable<Product> Sort(IQueryable<Product> query, ProductSortBy sortBy, bool sortDescending) =>
            (sortBy, sortDescending) switch
            {
                (ProductSortBy.Name, false) => query.OrderBy(p => p.Name).ThenBy(p => p.Id),
                (ProductSortBy.Name, true) => query.OrderByDescending(p => p.Name).ThenByDescending(p => p.Id),
                (ProductSortBy.CreatedAt, false) => query.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id),
                _ => query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            };
    }
}
