using ERP.Api;
using ERP.Api.Extensions;
using ERP.Application;
using ERP.Infrastructure;
using ERP.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddApplication()
    .AddPersistence(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

var app = builder.Build();

await app.SeedSuperAdminAsync();

// Configure the HTTP request pipeline.

app.UseExceptionHandler();

// En desarrollo el frontend llama por http://localhost:5117. Redirigir a HTTPS rompe el preflight
// de CORS: el navegador no acepta una redirección como respuesta a la consulta OPTIONS.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("AllowLocalhost");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Documentación de la API solo en desarrollo: /openapi/v1.json y /scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("AmirCorp ERP API")
        .AddPreferredSecuritySchemes("Bearer")
        );
}

app.Run();
