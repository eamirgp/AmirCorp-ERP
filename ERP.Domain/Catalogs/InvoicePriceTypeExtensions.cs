namespace ERP.Domain.Catalogs
{
    public static class InvoicePriceTypeExtensions
    {
        extension(InvoicePriceType invoicePriceType)
        {
            public string Description => invoicePriceType switch
            {
                InvoicePriceType.UnitValue => "Valor unitario",
                InvoicePriceType.UnitPrice => "Precio unitario",
                _ => invoicePriceType.ToString()
            };
        }
    }
}
