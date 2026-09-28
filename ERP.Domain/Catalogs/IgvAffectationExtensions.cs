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

            public decimal Rate => igvAffectation switch
            {
                IgvAffectation.Gravado => 0.18m,
                _ => 0m
            };
        }
    }
}
