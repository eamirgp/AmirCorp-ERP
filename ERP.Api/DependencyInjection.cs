using ERP.Api.Common;
using ERP.Api.Json;
using ERP.Api.Middleware;
using ERP.Api.OpenApi;
using ERP.Api.Services;
using ERP.Application.Contracts.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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
                    // Debe ir al final: atiende lo que los anteriores no reconocieron.
                    .AddExceptionHandler<UnexpectedExceptionHandler>()
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
                            .AllowAnyMethod()
                            // El frontend lee aquí el nombre de los archivos que descarga (plantillas, exportaciones).
                            .WithExposedHeaders("Content-Disposition");
                        });
                    })
                    .AddHttpContextAccessor()
                    .AddScoped<ICurrentUser, CurrentUser>()
                    .AddControllers()
                    .AddJsonOptions(options =>
                    {
                        // Los números viajan como números: "12.5" entre comillas se rechaza.
                        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
                        options.JsonSerializerOptions.Converters.Add(new TrimmingStringConverter());
                    })
                    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = InvalidModelStateResponse.Create);

                // El generador de OpenAPI lee estas opciones: deben coincidir con las de los controladores
                // para que documente los enums como texto y los números solo como números.
                services.ConfigureHttpJsonOptions(options =>
                {
                    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
                    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

                services.AddOpenApi(options =>
                {
                    options.AddDocumentTransformer((document, context, cancellationToken) =>
                    {
                        document.Info.Title = "AmirCorp ERP API";
                        return Task.CompletedTask;
                    });
                    // Con un convertidor propio, el generador no infiere el tipo: las contraseñas son texto.
                    options.AddSchemaTransformer((schema, context, cancellationToken) =>
                    {
                        if (context.JsonPropertyInfo?.CustomConverter is RawStringConverter)
                            schema.Type = JsonSchemaType.String | JsonSchemaType.Null;
                        return Task.CompletedTask;
                    });
                    options.AddSchemaTransformer<ComputedPropertiesTransformer>();
                    options.AddDocumentTransformer<BearerSecurityTransformer>();
                    options.AddOperationTransformer<BearerSecurityTransformer>();
                    options.AddOperationTransformer<ErrorResponseTransformer>();
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
                        // En cada pedido: el usuario sigue activo y se usa su rol actual (decisión 22).
                        options.Events = SessionValidation.Events();
                    });

                return services;
            }
        }
    }
}
