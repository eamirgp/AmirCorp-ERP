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
                    .AddApiRateLimits()
                    .AddCors(options =>
                    {
                        options.AddPolicy("AllowLocalhost", policy =>
                        {
                            policy
                            // Al publicar, la dirección real de la pantalla debe venir de la configuración (decisión 25).
                            .WithOrigins("http://localhost:5173")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            // La cookie del refresh token solo viaja si el origen está permitido con credenciales.
                            .AllowCredentials()
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
                        // Los enums solo como texto ("PEN"): un número como 99 no es un valor del catálogo.
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));
                        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
                        options.JsonSerializerOptions.Converters.Add(new TrimmingStringConverter());
                    })
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        options.InvalidModelStateResponseFactory = InvalidModelStateResponse.Create;
                        // Sin el "ProblemDetails" en inglés de ASP.NET (por ejemplo en un 415): esas respuestas salen
                        // sin cuerpo y StatusCodeResponse les pone { errors: [...] } en español.
                        options.SuppressMapClientErrors = true;
                    });

                // El generador de OpenAPI lee estas opciones: deben coincidir con las de los controladores
                // para que documente los enums como texto y los números solo como números.
                services.ConfigureHttpJsonOptions(options =>
                {
                    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
                    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));
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
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                            // Solo el algoritmo con que se firman, y 30 segundos de tolerancia en el reloj (por defecto
                            // son 5 minutos: un token de 15 minutos serviría 20).
                            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                            ClockSkew = TimeSpan.FromSeconds(30)
                        };
                        // En cada pedido: el usuario sigue activo y se usa su rol actual (decisión 22).
                        options.Events = SessionValidation.Events();
                    });

                return services;
            }
        }
    }
}
