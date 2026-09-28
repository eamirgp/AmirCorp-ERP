namespace ERP.Domain.Catalogs
{
    public static class CountryExtensions
    {
        extension(Country country)
        {
            public string Name => country switch
            {
                Country.PE => "Perú",
                Country.CN => "China",
                _ => country.ToString()
            };

            public bool IsPeru =>
                country is Country.PE;

            public string SunatCode => country switch
            {
                Country.PE => "9589",
                Country.CN => "9215",
                _ => country.ToString()
            };
        }
    }
}
