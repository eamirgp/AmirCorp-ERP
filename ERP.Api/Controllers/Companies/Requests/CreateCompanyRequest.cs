using ERP.Application.Features.Companies.CreateCompany;
using ERP.Domain.Companies;

namespace ERP.Api.Controllers.Companies.Requests
{
    public sealed record CreateCompanyRequest(
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

        public CreateCompanyDto ToDto() =>
            new(Ruc!, Name!);
    }
}
