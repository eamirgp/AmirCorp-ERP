using ERP.Application.Contracts.Infrastructure;
using ERP.Infrastructure.Services.Auth;
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
                    .AddSingleton<IProductSpreadsheet, ProductSpreadsheet>();

                return services;
            }
        }
    }
}
