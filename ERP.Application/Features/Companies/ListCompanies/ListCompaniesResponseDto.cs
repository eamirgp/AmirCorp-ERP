namespace ERP.Application.Features.Companies.ListCompanies
{
    public sealed record ListCompaniesResponseDto(
        Guid Id,
        string Ruc,
        string Name,
        bool IsActive,
        // Versión de la empresa: el formulario la devuelve al editar para no pisar cambios de otra persona.
        uint RowVersion
        );
}
