namespace ERP.Application.Common.Pagination
{
    public static class PaginationDefaults
    {
        public const int MinPageSize = 5;
        public const int MaxPageSize = 20;
        public const int DefaultPageSize = 10;

        public static int NormalizedPage(int? page) =>
            page is null or < 1 ? 1 : page.Value;

        public static int NormalizedPageSize(int? pageSize) =>
            Math.Clamp(pageSize ?? DefaultPageSize, MinPageSize, MaxPageSize);
    }
}
