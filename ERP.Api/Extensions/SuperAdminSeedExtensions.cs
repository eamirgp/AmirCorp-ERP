using ERP.Application.Features.Users.EnsureSuperAdmin;
using ERP.Domain.Common;

namespace ERP.Api.Extensions
{
    public static class SuperAdminSeedExtensions
    {
        private const string SectionName = "SuperAdmin";

        extension(WebApplication app)
        {
            /// <summary>
            /// Crea el SuperAdmin al iniciar la API si todavía no existe, usando la sección "SuperAdmin" de la configuración.
            /// </summary>
            public async Task SeedSuperAdminAsync()
            {
                var section = app.Configuration.GetSection(SectionName);

                var request = new EnsureSuperAdminDto(
                    section["Name"],
                    section["Email"],
                    section["Password"]
                    );

                using var scope = app.Services.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<IEnsureSuperAdminUseCase>();

                try
                {
                    var result = await useCase.ExecuteAsync(request);

                    if (!result.IsSuccess)
                        app.Logger.LogWarning(
                            "No se creó el SuperAdmin. Revisa la sección '{Section}' de la configuración: {Errors}",
                            SectionName,
                            string.Join(" ", result.Errors)
                            );
                    else if (result.Value)
                        app.Logger.LogInformation("SuperAdmin creado.");
                }
                catch (DomainException ex)
                {
                    app.Logger.LogWarning(
                        "No se creó el SuperAdmin. Revisa la sección '{Section}' de la configuración: {Error}",
                        SectionName,
                        ex.Message
                        );
                }
            }
        }
    }
}
