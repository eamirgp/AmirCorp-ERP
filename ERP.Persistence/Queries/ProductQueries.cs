using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Products.ExportProducts;
using ERP.Application.Features.Products.GetProduct;
using ERP.Application.Features.Products.ListProducts;
using ERP.Application.Features.Products.SupplierCodes;
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
            var query = Filter(_context.Products.AsNoTracking(), listProductsDto.SearchTerm, listProductsDto.IsActive, listProductsDto.SupplierId);

            // En una compra, la primera búsqueda es entre lo que ya se le compra a ese proveedor. Un proveedor que
            // todavía no está registrado (se crea con la compra) no tiene ninguno.
            if (listProductsDto.OnlySupplierProducts)
            {
                var onlySupplierId = listProductsDto.SupplierId;
                query = onlySupplierId is null
                    ? query.Where(p => false)
                    : query.Where(p => p.SupplierCodes.Any(c => c.SupplierId == onlySupplierId));
            }

            var totalCount = await query.CountAsync();

            // Si la página pedida ya no existe, se devuelve la última.
            var page = PaginationDefaults.ClampPage(listProductsDto.Page, listProductsDto.PageSize, totalCount);

            var (hasTerm, codeTerm, nameTerm) = SearchTerms(listProductsDto.SearchTerm);
            var supplierId = listProductsDto.SupplierId;

            var rows = await Sort(query, listProductsDto.SortBy, listProductsDto.SortDescending)
                .Skip((page - 1) * listProductsDto.PageSize)
                .Take(listProductsDto.PageSize)
                .Select(p => new
                {
                    Product = p,
                    UnitOfMeasureName = _context.UnitsOfMeasure.Where(u => u.Code == p.UnitOfMeasureCode).Select(u => u.Name).First(),
                    SupplierCodes = _context.ProductSupplierCodes
                        .Where(c => c.ProductId == p.Id)
                        .Join(_context.BusinessPartners, c => c.SupplierId, s => s.Id, (c, s) => new { c.SupplierId, SupplierName = s.Name, s.DocumentNumber, c.Code })
                        .OrderBy(x => x.SupplierName)
                        .Select(x => new ProductSupplierCodeResponseDto(x.SupplierId, x.SupplierName, x.DocumentNumber, x.Code))
                        .ToList(),
                    SupplierCode = supplierId == null
                        ? null
                        : p.SupplierCodes.Where(c => c.SupplierId == supplierId).Select(c => c.Code).FirstOrDefault(),
                    // Solo si no coincidió el código interno ni el nombre: el proveedor del primer código que coincide
                    // (en una compra, solo puede ser el de su proveedor).
                    MatchedSupplierId = !hasTerm || p.Code.Contains(codeTerm) || EF.Functions.Unaccent(p.Name.ToLower()).Contains(nameTerm)
                        ? null
                        : _context.ProductSupplierCodes
                            .Where(c => c.ProductId == p.Id && (supplierId == null || c.SupplierId == supplierId) && c.Code.Contains(codeTerm))
                            .Join(_context.BusinessPartners, c => c.SupplierId, s => s.Id, (c, s) => new { c.SupplierId, s.Name })
                            .OrderBy(x => x.Name)
                            .Select(x => (Guid?)x.SupplierId)
                            .FirstOrDefault(),
                    RowVersion = EF.Property<uint>(p, "RowVersion")
                })
                .ToArrayAsync();

            var items = rows
                .Select(r => new ListProductsResponseDto(
                    r.Product.Id,
                    r.Product.Code,
                    r.Product.Name,
                    r.Product.UnitOfMeasureCode,
                    r.UnitOfMeasureName,
                    r.Product.IgvAffectation,
                    r.Product.SalePrice,
                    r.Product.IsActive,
                    r.SupplierCodes,
                    r.SupplierCode,
                    r.MatchedSupplierId,
                    r.RowVersion,
                    listProductsDto.LinkCode
                    ))
                .ToArray();

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
                p.UnitOfMeasureCode,
                _context.UnitsOfMeasure.Where(u => u.Code == p.UnitOfMeasureCode).Select(u => u.Name).First(),
                p.IgvAffectation,
                p.SalePrice,
                p.IsActive,
                _context.ProductSupplierCodes
                    .Where(c => c.ProductId == p.Id)
                    .Join(_context.BusinessPartners, c => c.SupplierId, s => s.Id, (c, s) => new { c.SupplierId, SupplierName = s.Name, s.DocumentNumber, c.Code })
                    .OrderBy(x => x.SupplierName)
                    .Select(x => new ProductSupplierCodeResponseDto(x.SupplierId, x.SupplierName, x.DocumentNumber, x.Code))
                    .ToList(),
                p.CreatedAt,
                _context.Users.Where(u => u.Id == p.CreatedBy).Select(u => u.Name).FirstOrDefault(),
                p.UpdatedAt,
                _context.Users.Where(u => u.Id == p.UpdatedBy).Select(u => u.Name).FirstOrDefault()
                ))
            .FirstOrDefaultAsync();

        public async Task<IReadOnlyCollection<ProductExportRowDto>> ListForExportAsync(ExportProductsDto exportProductsDto) =>
            await Sort(Filter(_context.Products.AsNoTracking(), exportProductsDto.SearchTerm, exportProductsDto.IsActive), exportProductsDto.SortBy, exportProductsDto.SortDescending)
            .Select(p => new ProductExportRowDto(
                p.Code,
                p.Name,
                _context.UnitsOfMeasure.Where(u => u.Code == p.UnitOfMeasureCode).Select(u => u.Name).First(),
                p.IgvAffectation,
                p.SalePrice
                ))
            .ToArrayAsync();

        // La lista y la exportación filtran y ordenan igual: el Excel trae exactamente lo que se ve en la pantalla.
        // El texto se busca en cualquier parte del código interno, del nombre o de los códigos de proveedores. En una
        // compra (con proveedor) solo cuentan los códigos de ese proveedor: el mismo código de otro es otra cosa.
        private static IQueryable<Product> Filter(IQueryable<Product> query, string? searchTerm, bool? isActive, Guid? supplierId = null)
        {
            var (hasTerm, codeTerm, nameTerm) = SearchTerms(searchTerm);
            if (hasTerm)
            {
                query = query
                    .Where(p =>
                    p.Code.Contains(codeTerm) ||
                    EF.Functions.Unaccent(p.Name.ToLower()).Contains(nameTerm) ||
                    p.SupplierCodes.Any(c => (supplierId == null || c.SupplierId == supplierId) && c.Code.Contains(codeTerm))
                    );
            }

            if (isActive is not null)
                query = query
                    .Where(p => p.IsActive == isActive);

            return query;
        }

        /// <summary>El texto buscado como se compara: en mayúsculas para los códigos y en minúsculas sin tildes para el nombre.</summary>
        private static (bool HasTerm, string CodeTerm, string NameTerm) SearchTerms(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return (false, "", "");

            var term = searchTerm.Trim();
            return (true, Product.NormalizeCode(term), SearchText.Normalize(term));
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
