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

            /// <summary>El símbolo que va delante de un monto: "S/ 5.00", "US$ 5.00".</summary>
            public string Symbol => currency switch
            {
                Currency.PEN => "S/",
                Currency.USD => "US$",
                _ => currency.ToString()
            };

            /// <summary>Si SUNAT publica su tipo de cambio: solo el dólar (el sol no lo necesita).</summary>
            public bool HasPublishedExchangeRate => currency is Currency.USD;
        }
    }
}
