namespace ERP.Domain.Catalogs
{
    public static class UnitOfMeasureExtensions
    {
        extension(UnitOfMeasure unitOfMeasure)
        {
            public string Description => unitOfMeasure switch
            {
                UnitOfMeasure.NIU => "Unidad",
                UnitOfMeasure.C62 => "Pieza",
                UnitOfMeasure.DZN => "Docena",
                UnitOfMeasure.BX => "Caja",
                _ => unitOfMeasure.ToString(),
            };

            public decimal? FixedConversionFactor => unitOfMeasure switch
            {
                UnitOfMeasure.NIU => 1,
                UnitOfMeasure.C62 => 1,
                UnitOfMeasure.DZN => 12,
                UnitOfMeasure.BX => null,
                _ => null
            };
        }
    }
}
