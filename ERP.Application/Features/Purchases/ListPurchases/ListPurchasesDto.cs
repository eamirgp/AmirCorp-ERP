namespace ERP.Application.Features.Purchases.ListPurchases
{
    public sealed record ListPurchasesDto(
        int Page,
        int PageSize,
        string? SearchTerm,
        Guid? CompanyId,
        PurchaseSortBy SortBy,
        bool SortDescending
        );
}
