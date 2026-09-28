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
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Ruc))
                errors.Add("El RUC es requerido.");
            else if (Ruc.Length != Company.RucLength || !Ruc.All(char.IsDigit))
                errors.Add($"El RUC debe tener {Company.RucLength} dígitos.");

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("El nombre es requerido.");
            else if (Name.Length > Company.NameMaxLength)
                errors.Add($"El nombre no puede exceder los {Company.NameMaxLength} caracteres.");

            return errors;
        }

        public CreateCompanyDto ToDto() =>
            new(Ruc!, Name!);
    }
}
