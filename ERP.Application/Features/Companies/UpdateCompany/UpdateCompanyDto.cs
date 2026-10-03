namespace ERP.Application.Features.Companies.UpdateCompany
{
    public sealed record UpdateCompanyDto(
        Guid Id,
        string Ruc,
        string Name,
        // Versión que se abrió en el formulario.
        uint RowVersion
        );
}
