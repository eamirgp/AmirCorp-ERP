namespace ERP.Application.Features.Companies.GetCompany
{
    public sealed record GetCompanyResponseDto(
        Guid Id,
        string Ruc,
        string Name,
        bool IsActive,
        DateTime CreatedAt,
        string? CreatedByName,
        DateTime? UpdatedAt,
        string? UpdatedByName
        );
}
