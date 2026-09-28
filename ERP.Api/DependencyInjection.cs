using ERP.Api.Json;
using ERP.Api.Middleware;
using ERP.Api.Services;
using ERP.Application.Contracts.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

namespace ERP.Api
{
    public static class DependencyInjection
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddApi(IConfiguration configuration)
            {
                services
                    .AddExceptionHandler<DomainExceptionHandler>()
                    .AddExceptionHandler<ConcurrencyExceptionHandler>()
                    .AddProblemDetails()
                    .AddCors(options =>
                    {
                        options.AddPolicy("AllowLocalhost", policy =>
                        {
                            policy
                            .WithOrigins(
                                "http://localhost:5173",
                                "https://poster-caption-endorphin.ngrok-free.dev"
                                )
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                        });
                    })
                    .AddHttpContextAccessor()
                    .AddScoped<ICurrentUser, CurrentUser>()
                    .AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
                        options.JsonSerializerOptions.Converters.Add(new TrimmingStringConverter());
                    });

                var jwtSecret = configuration["JwtSettings:Secret"];

                if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
                    throw new InvalidOperationException(
                        "Falta 'JwtSettings:Secret' o tiene menos de 32 caracteres. Configúralo en User Secrets (desarrollo) o en las variables de entorno (producción)."
                        );

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = configuration["JwtSettings:Issuer"],
                            ValidAudience = configuration["JwtSettings:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                        };
                    });

                return services;
            }
        }
    }
}
