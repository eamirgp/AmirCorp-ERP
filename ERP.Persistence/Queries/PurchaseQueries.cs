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
                var raw = listPurchasesDto.SearchTerm.Trim();
                var documentTerm = Purchase.NormalizeSerie(raw);
                var nameTerm = SearchText.Normalize(raw);

                // El número se guarda con ceros a la izquierda ("00000123"): "123" y "F001-123" deben encontrarlo.
                var (serieTerm, numberTerm) = SplitDocument(documentTerm);

                query = query
                    .Where(p =>
                    (p.Serie + "-" + p.Number).StartsWith(documentTerm) ||
                    (numberTerm != null && p.Number == numberTerm && (serieTerm == null || p.Serie == serieTerm)) ||
                    p.SupplierDocumentNumber.StartsWith(raw) ||
                    EF.Functions.Unaccent(p.SupplierName.ToLower()).Contains(nameTerm)
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

        /// <summary>
        /// "123" → (null, "00000123"); "F001-123" → ("F001", "00000123"); otro texto → (null, null). El número se
        /// completa con ceros como se guarda, para compararlo exacto.
        /// </summary>
        private static (string? Serie, string? Number) SplitDocument(string term)
        {
            var parts = term.Split('-', 2);
            var (serie, number) = parts.Length == 2 ? (parts[0], parts[1]) : (null, parts[0]);

            if (number.Length == 0 || number.Length > Purchase.NumberMaxLength || !number.All(char.IsAsciiDigit))
                return (null, null);

            return (string.IsNullOrEmpty(serie) ? null : serie, Purchase.NormalizeNumber(number));
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
                    l.InvoiceUnitOfMeasureCode,
                    _context.UnitsOfMeasure.Where(u => u.Code == l.InvoiceUnitOfMeasureCode).Select(u => u.Name).First(),
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
