using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Purchases.GetPurchase;
using ERP.Application.Features.Purchases.ListPurchases;
using ERP.Domain.Catalogs;
using ERP.Domain.Purchases;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Queries
{
    internal sealed class PurchaseQueries : IPurchaseQueries
    {
        private readonly ErpDbContext _context;

        public PurchaseQueries(ErpDbContext context) => _context = context;

        public async Task<SortedPagedResult<ListPurchasesResponseDto, PurchaseSortBy>> ListPurchasesAsync(ListPurchasesDto listPurchasesDto)
        {
            var query = _context.Purchases.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(listPurchasesDto.SearchTerm))
            {
                var documentTerm = Purchase.NormalizeSerie(listPurchasesDto.SearchTerm);
                var nameTerm = listPurchasesDto.SearchTerm.ToLower();

                query = query
                    .Where(p =>
                    (p.Serie + "-" + p.Number).StartsWith(documentTerm) ||
                    p.Number.StartsWith(listPurchasesDto.SearchTerm) ||
                    p.SupplierDocumentNumber.StartsWith(listPurchasesDto.SearchTerm) ||
                    p.SupplierName.ToLower().Contains(nameTerm)
                    );
            }

            if (listPurchasesDto.CompanyId is not null)
                query = query.Where(p => p.CompanyId == listPurchasesDto.CompanyId);

            var totalCount = await query.CountAsync();

            // Si la página pedida ya no existe, se devuelve la última.
            var page = PaginationDefaults.ClampPage(listPurchasesDto.Page, listPurchasesDto.PageSize, totalCount);

            query = (listPurchasesDto.SortBy, listPurchasesDto.SortDescending) switch
            {
                (PurchaseSortBy.IssueDate, false) => query.OrderBy(p => p.IssueDate).ThenBy(p => p.Id),
                (PurchaseSortBy.IssueDate, true) => query.OrderByDescending(p => p.IssueDate).ThenByDescending(p => p.Id),
                (PurchaseSortBy.SupplierName, false) => query.OrderBy(p => p.SupplierName).ThenBy(p => p.Id),
                (PurchaseSortBy.SupplierName, true) => query.OrderByDescending(p => p.SupplierName).ThenByDescending(p => p.Id),
                (PurchaseSortBy.Total, false) => query.OrderBy(p => p.Total).ThenBy(p => p.Id),
                (PurchaseSortBy.Total, true) => query.OrderByDescending(p => p.Total).ThenByDescending(p => p.Id),
                (PurchaseSortBy.CreatedAt, false) => query.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id),
                _ => query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            };

            var items = await query
                .Skip((page - 1) * listPurchasesDto.PageSize)
                .Take(listPurchasesDto.PageSize)
                .Select(p => new ListPurchasesResponseDto(
                    p.Id,
                    p.CompanyId,
                    _context.Companies.Where(c => c.Id == p.CompanyId).Select(c => c.Name).FirstOrDefault(),
                    p.TaxDocumentType,
                    p.Serie,
                    p.Number,
                    p.IssueDate,
                    p.Currency,
                    p.ExchangeRate,
                    p.SupplierDocumentNumber,
                    p.SupplierName,
                    p.Total,
                    p.IsCancelled,
                    p.CancellationReason
                    ))
                .ToArrayAsync();

            return new SortedPagedResult<ListPurchasesResponseDto, PurchaseSortBy>(
                items,
                page,
                listPurchasesDto.PageSize,
                totalCount,
                listPurchasesDto.SortBy,
                listPurchasesDto.SortDescending
                );
        }

        public async Task<GetPurchaseResponseDto?> GetPurchaseAsync(GetPurchaseDto getPurchaseDto) =>
            await _context.Purchases
            .AsNoTracking()
            .Where(p => p.Id == getPurchaseDto.Id)
            .Select(p => new GetPurchaseResponseDto(
                p.Id,
                p.CompanyId,
                _context.Companies.Where(c => c.Id == p.CompanyId).Select(c => c.Name).FirstOrDefault(),
                p.TaxDocumentType,
                p.Serie,
                p.Number,
                p.IssueDate,
                p.Currency,
                p.ExchangeRate,
                p.InvoicePriceType,
                p.SupplierId,
                p.SupplierIdentityDocumentType,
                p.SupplierDocumentNumber,
                p.SupplierName,
                p.TotalBaseAmount,
                p.TotalIgvAmount,
                p.Total,
                p.IsCancelled,
                p.CancellationReason,
                p.CreatedAt,
                _context.Users.Where(u => u.Id == p.CreatedBy).Select(u => u.Name).FirstOrDefault(),
                p.UpdatedAt,
                _context.Users.Where(u => u.Id == p.UpdatedBy).Select(u => u.Name).FirstOrDefault(),
                p.Lines
                .OrderBy(l => l.LineNumber)
                .Select(l => new GetPurchaseLineResponseDto(
                    l.LineNumber,
                    l.ProductId,
                    l.ProductCode,
                    l.ProductName,
                    l.InvoiceIgvAffectation,
                    l.InvoiceUnitOfMeasure,
                    l.InvoiceQuantity,
                    l.InvoicePriceType == InvoicePriceType.UnitValue
                        ? l.InvoiceUnitValue
                        : l.InvoiceUnitPrice,
                    l.BaseAmount,
                    l.IgvAmount,
                    l.Total
                    )).ToArray()
                ))
            .FirstOrDefaultAsync();
    }
}
