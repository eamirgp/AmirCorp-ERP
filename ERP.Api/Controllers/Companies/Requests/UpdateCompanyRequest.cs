using ERP.Application.Features.Companies.UpdateCompany;
using ERP.Domain.Companies;

namespace ERP.Api.Controllers.Companies.Requests
{
    public sealed record UpdateCompanyRequest(
        string? Ruc,
        string? Name
        )
    {
        public IReadOnlyCollection<string> Validate()
        {
            // Las mismas reglas del dominio, revisadas antes para avisar todos los errores juntos.
            var errors = new List<string>();

            if (Company.RucError(Ruc) is { } rucError)
                errors.Add(rucError);

            if (Company.NameError(Name) is { } nameError)
                errors.Add(nameError);

            return errors;
        }

        public UpdateCompanyDto ToDto(Guid id) =>
            new(id, Ruc!, Name!);
    }
}
