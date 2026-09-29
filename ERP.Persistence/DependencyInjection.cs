using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Persistence.Commands;
using ERP.Persistence.Context;
using ERP.Persistence.Interceptors;
using ERP.Persistence.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Persistence
{
    public static class DependencyInjection
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddPersistence(IConfiguration configuration)
            {
                var connectionString = configuration.GetConnectionString("DB");

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException(
                        "Falta 'ConnectionStrings:DB'. Configúralo en User Secrets (desarrollo) o en las variables de entorno (producción). " +
                        "Formato: Host=localhost;Port=5432;Database=erp;Username=postgres;Password=..."
                        );

                services
                    .AddScoped<AuditInterceptor>()
                    .AddDbContext<ErpDbContext>((sp, options) =>
                    {
                        options.UseNpgsql(connectionString);
                        options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
                    })
                    .AddScoped<IUserRepository, UserRepository>()
                    .AddScoped<IUserQueries, UserQueries>()

                    .AddScoped<ICompanyRepository, CompanyRepository>()
                    .AddScoped<ICompanyQueries, CompanyQueries>()

                    .AddScoped<IProductRepository, ProductRepository>()
                    .AddScoped<IProductQueries, ProductQueries>()

                    .AddScoped<IBusinessPartnerRepository, BusinessPartnerRepository>()
                    .AddScoped<IBusinessPartnerQueries, BusinessPartnerQueries>()

                    .AddScoped<IStockEntryRepository, StockEntryRepository>()

                    .AddScoped<IPurchaseRepository, PurchaseRepository>()
                    .AddScoped<IPurchaseQueries, PurchaseQueries>()

                    .AddScoped<IAuditQueries, AuditQueries>()

                    .AddScoped<ISavedViewRepository, SavedViewRepository>()
                    .AddScoped<ISavedViewQueries, SavedViewQueries>()
                    .AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>()
                    .AddScoped<IUnitOfMeasureQueries, UnitOfMeasureQueries>()

                    .AddScoped<IUnitOfWork, UnitOfWork>();

                return services;
            }
        }
    }
}
