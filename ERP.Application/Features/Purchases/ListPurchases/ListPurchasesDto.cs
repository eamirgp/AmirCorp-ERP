namespace ERP.Application.Features.Purchases.ListPurchases
{
    public sealed record ListPurchasesDto(
        int Page,
        int PageSize,
        string? SearchTerm,
        Guid? CompanyId,
        PurchaseSortBy SortBy,
        bool SortDescending
        )
    {
        /// <summary>Orden de la lista cuando la pantalla no pide uno: por fecha de emisión, las más recientes primero.</summary>
        public const PurchaseSortBy DefaultSortBy = PurchaseSortBy.IssueDate;
        public const bool DefaultSortDescending = true;
    }
}
