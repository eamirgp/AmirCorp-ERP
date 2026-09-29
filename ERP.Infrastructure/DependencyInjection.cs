using ERP.Application.Contracts.Infrastructure;
using ERP.Infrastructure.Services.Auth;
using ERP.Infrastructure.Services.RucLookup;
using ERP.Infrastructure.Services.Settings;
using ERP.Infrastructure.Services.Spreadsheets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Infrastructure
{
    public static class DependencyInjection
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddInfrastructure(IConfiguration configuration)
            {
                services
                    .AddSingleton<IPasswordService, PasswordService>()
                    .Configure<JwtSettings>(configuration.GetSection("JwtSettings"))
                    .AddSingleton<IJwtService, JwtService>()
                    .AddSingleton<IProductSpreadsheet, ProductSpreadsheet>()
                    // Consulta de RUC: sin RucLookup:Token la consulta no está disponible y el sistema funciona igual.
                    .Configure<RucLookupSettings>(configuration.GetSection("RucLookup"))
                    .AddSingleton<IRucLookup, DecolectaRucLookup>();

                return services;
            }
        }
    }
}
