using ERP.Application.Common.Pagination;

namespace ERP.Api.Common
{
    /// <summary>Búsqueda y estado de una lista corta (empresas, usuarios, unidades). Sin filtros trae todo.</summary>
    /// <param name="SearchTerm">Texto a buscar, sin distinguir mayúsculas ni tildes.</param>
    /// <param name="IsActive">true solo activos, false solo inactivos; sin valor, todos.</param>
    public sealed record ListFilterRequest(string? SearchTerm, bool? IsActive)
    {
        public ListFilterDto ToDto() => new(string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm, IsActive);
    }
}
