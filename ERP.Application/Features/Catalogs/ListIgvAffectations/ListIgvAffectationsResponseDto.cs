using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListIgvAffectations
{
    public sealed record ListIgvAffectationsResponseDto(
        IgvAffectation IgvAffectation,
        string Description
        )
    {
        /// <summary>Nombre corto ("Gravado") para las tablas donde el nombre completo de SUNAT no cabe.</summary>
        public string ShortDescription => IgvAffectation.ShortDescription;
    }
}
