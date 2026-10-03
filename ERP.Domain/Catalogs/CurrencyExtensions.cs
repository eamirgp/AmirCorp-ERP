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

            /// <summary>Si SUNAT publica su tipo de cambio: solo el dólar (el sol no lo necesita).</summary>
            public bool HasPublishedExchangeRate => currency is Currency.USD;
        }
    }
}
