using ERP.Api;
using ERP.Api.Extensions;
using ERP.Application;
using ERP.Infrastructure;
using ERP.Persistence;

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
app.UseHttpsRedirection();
app.UseCors("AllowLocalhost");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
