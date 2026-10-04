namespace ERP.Application.Common.Pagination
{
    /// <summary>
    /// Página de una lista ordenable, con el orden que se aplicó. Si la pantalla no pidió orden, la API usa
    /// el suyo por defecto y lo informa aquí, así la pantalla muestra el orden real sin decidirlo ella.
    /// También informa cuál es ese orden por defecto: si el usuario vuelve a él, la pantalla lo quita de la URL y la
    /// lista se reconoce igual a la que abre normalmente (o a una vista guardada sin orden).
    /// </summary>
    public sealed class SortedPagedResult<T, TSortBy> : PagedResult<T>
        where TSortBy : struct, Enum
    {
        public TSortBy SortBy { get; }
        public bool SortDescending { get; }
        public TSortBy DefaultSortBy { get; }
        public bool DefaultSortDescending { get; }

        public SortedPagedResult(
            IReadOnlyCollection<T> items,
            int page,
            int pageSize,
            int totalCount,
            TSortBy sortBy,
            bool sortDescending,
            TSortBy defaultSortBy,
            bool defaultSortDescending)
            : base(items, page, pageSize, totalCount)
        {
            SortBy = sortBy;
            SortDescending = sortDescending;
            DefaultSortBy = defaultSortBy;
            DefaultSortDescending = defaultSortDescending;
        }
    }
}
