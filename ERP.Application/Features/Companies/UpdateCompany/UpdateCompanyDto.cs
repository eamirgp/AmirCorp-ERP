namespace ERP.Application.Features.Companies.UpdateCompany
{
    public sealed record UpdateCompanyDto(
        Guid Id,
        string Ruc,
        string Name
        );
}
