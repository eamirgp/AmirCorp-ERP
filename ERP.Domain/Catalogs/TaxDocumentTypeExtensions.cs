namespace ERP.Domain.Catalogs
{
    public static class TaxDocumentTypeExtensions
    {
        extension(TaxDocumentType taxDocumentType)
        {
            public string Description => taxDocumentType switch
            {
                TaxDocumentType.Boleta => "Boleta",
                TaxDocumentType.Factura =>"Factura",
                _ => taxDocumentType.ToString()
            };
        }
    }
}
