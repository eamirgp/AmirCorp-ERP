using ERP.Domain.Common;

namespace ERP.Domain.Companies
{
    public sealed class Company : AuditableEntity
    {
        public const int RucLength = 11;
        // Igual que clientes y proveedores: una razón social de SUNAT puede pasar de 100 caracteres.
        public const int NameMaxLength = 200;

        public string Ruc { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }

        private Company(Guid id, string ruc, string name, bool isActive) : base(id)
        {
            Ruc = ruc;
            Name = name;
            IsActive = isActive;
        }

        public static Company Create(string ruc, string name) =>
            new(Guid.CreateVersion7(), ValidateRuc(ruc), ValidateName(name), isActive: true);

        public void UpdateRuc(string ruc) =>
            Ruc = ValidateRuc(ruc);

        public void UpdateName(string name) =>
            Name = ValidateName(name);

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        private static string ValidateRuc(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc))
                throw new DomainException("El RUC es requerido.");

            if (ruc.Length != RucLength || !ruc.All(char.IsDigit))
                throw new DomainException($"El RUC debe tener {RucLength} dígitos.");

            return ruc;
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre es requerido.");

            if (name.Length > NameMaxLength)
                throw new DomainException($"El nombre no puede exceder los {NameMaxLength} caracteres.");

            return name;
        }
    }
}
