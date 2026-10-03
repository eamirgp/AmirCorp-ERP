namespace ERP.Application.Common.Pagination
{
    /// <summary>
    /// Filtro de las listas cortas que no se paginan (empresas, usuarios, unidades): el texto buscado y el estado. Sin
    /// filtros trae todo. La búsqueda la hace la API, igual que en las listas paginadas: sin distinguir mayúsculas ni tildes.
    /// </summary>
    public sealed record ListFilterDto(string? SearchTerm, bool? IsActive)
    {
        public static readonly ListFilterDto None = new(null, null);
    }
}
