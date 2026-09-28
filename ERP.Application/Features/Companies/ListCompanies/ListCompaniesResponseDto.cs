namespace ERP.Application.Features.Companies.ListCompanies
{
    public sealed record ListCompaniesResponseDto(
        Guid Id,
        string Ruc,
        string Name,
        bool IsActive
        );
}
