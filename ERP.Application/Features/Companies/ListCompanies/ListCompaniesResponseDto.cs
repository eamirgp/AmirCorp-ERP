namespace ERP.Application.Features.Companies.ListCompanies
{
    public sealed record ListCompaniesResponseDto(
        Guid Id,
        string Ruc,
        string Name,
        bool IsActive,
        // Versión de la empresa: el formulario la devuelve al editar para no pisar cambios de otra persona.
        uint RowVersion
        )
    {
        /// <summary>"Activa" o "Inactiva", para la columna Estado.</summary>
        public string StatusDescription => IsActive ? "Activa" : "Inactiva";
    }
}
