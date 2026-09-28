namespace ERP.Domain.Catalogs
{
    public static class CurrencyExtensions
    {
        extension(Currency currency)
        {
            public string Description => currency switch
            {
                Currency.PEN => "Soles",
                Currency.USD => "Dólares",
                _ => currency.ToString()
            };
        }
    }
}
