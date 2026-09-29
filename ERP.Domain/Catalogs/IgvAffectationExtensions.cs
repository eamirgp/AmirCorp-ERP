namespace ERP.Domain.Catalogs
{
    public static class IgvAffectationExtensions
    {
        extension(IgvAffectation igvAffectation)
        {
            public string Description => igvAffectation switch
            {
                IgvAffectation.Gravado => "Gravado - Operación Onerosa",
                IgvAffectation.Inafecto => "Inafecto - Operación Onerosa",
                _ => igvAffectation.ToString()
            };

            /// <summary>Nombre corto para las tablas, donde el nombre completo de SUNAT se repite en cada fila.</summary>
            public string ShortDescription => igvAffectation switch
            {
                IgvAffectation.Gravado => "Gravado",
                IgvAffectation.Inafecto => "Inafecto",
                _ => igvAffectation.ToString()
            };

            public decimal Rate => igvAffectation switch
            {
                IgvAffectation.Gravado => 0.18m,
                _ => 0m
            };
        }
    }
}
