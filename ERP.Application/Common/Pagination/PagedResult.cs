namespace ERP.Application.Common.Pagination
{
    public class PagedResult<T>
    {
        public IReadOnlyCollection<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages => (int)Math.Ceiling(TotalCount/(double)PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;

        /// <summary>Posición del primer registro de la página (1 en la primera). 0 si la lista está vacía.</summary>
        public int From => Items.Count == 0 ? 0 : (Page - 1) * PageSize + 1;

        /// <summary>Posición del último registro de la página. 0 si la lista está vacía.</summary>
        public int To => Items.Count == 0 ? 0 : From + Items.Count - 1;

        public IReadOnlyList<int> PageSizeOptions => PaginationDefaults.PageSizeOptions;

        public PagedResult(IReadOnlyCollection<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
