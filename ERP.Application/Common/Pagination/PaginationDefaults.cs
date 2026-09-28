namespace ERP.Application.Common.Pagination
{
    public static class PaginationDefaults
    {
        public const int MinPageSize = 5;
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 20;

        /// <summary>Tamaños de página que ofrece la pantalla. Se envían en cada lista paginada.</summary>
        public static readonly IReadOnlyList<int> PageSizeOptions = [10, 20, 50, 100];

        public static int NormalizedPage(int? page) =>
            page is null or < 1 ? 1 : page.Value;

        public static int NormalizedPageSize(int? pageSize) =>
            Math.Clamp(pageSize ?? DefaultPageSize, MinPageSize, MaxPageSize);
    }
}
