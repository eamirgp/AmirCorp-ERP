namespace ERP.Application.Common.Pagination
{
    /// <summary>
    /// Página de una lista ordenable, con el orden que se aplicó. Si la pantalla no pidió orden, la API usa
    /// el suyo por defecto y lo informa aquí, así la pantalla muestra el orden real sin decidirlo ella.
    /// </summary>
    public sealed class SortedPagedResult<T, TSortBy> : PagedResult<T>
        where TSortBy : struct, Enum
    {
        public TSortBy SortBy { get; }
        public bool SortDescending { get; }

        public SortedPagedResult(IReadOnlyCollection<T> items, int page, int pageSize, int totalCount, TSortBy sortBy, bool sortDescending)
            : base(items, page, pageSize, totalCount)
        {
            SortBy = sortBy;
            SortDescending = sortDescending;
        }
    }
}
