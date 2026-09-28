using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListIgvAffectations
{
    public sealed record ListIgvAffectationsResponseDto(
        IgvAffectation IgvAffectation,
        string Description
        );
}
