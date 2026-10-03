using ERP.Domain.Common;
using ERP.Domain.Partners.Enums;

namespace ERP.Domain.Companies
{
    public sealed class Company : AuditableEntity
    {
        public const int RucLength = IdentityDocumentTypeExtensions.RucLength;
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

        /// <summary>
        /// Corrige el RUC. Solo mientras la empresa no tenga compras: el RUC identifica al contribuyente, y cambiarlo
        /// con compras registradas mezclaría dos empresas. Cada compra guarda su propia copia del RUC.
        /// </summary>
        /// <param name="hasPurchases">Si la empresa ya tiene compras registradas (también las anuladas).</param>
        public void UpdateRuc(string ruc, bool hasPurchases)
        {
            var normalized = ValidateRuc(ruc);
            if (RucChangeError(normalized, hasPurchases) is { } error)
                throw new DomainException(error);

            Ruc = normalized;
        }

        public void UpdateName(string name) =>
            Name = ValidateName(name);

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        /// <summary>El RUC tal como se guarda: sin espacios.</summary>
        public static string NormalizeRuc(string ruc) =>
            IdentityDocumentTypeExtensions.NormalizeDocumentNumber(ruc);

        /// <summary>
        /// Qué tiene de malo el RUC, o null si es válido. Se valida como el de un proveedor: 11 dígitos, prefijo y dígito
        /// verificador. La usan el dominio y la API, que avisa todos los errores juntos.
        /// </summary>
        public static string? RucError(string? ruc) =>
            string.IsNullOrWhiteSpace(ruc)
                ? "El RUC es requerido."
                : IdentityDocumentType.Ruc.DocumentNumberError(NormalizeRuc(ruc));

        /// <summary>La razón social tal como se guarda: sin espacios al inicio ni al final, ni dobles en medio.</summary>
        public static string NormalizeName(string name) =>
            TextNormalizer.CollapseSpaces(name);

        /// <summary>Qué tiene de malo la razón social, o null si está bien.</summary>
        public static string? NameError(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "La razón social es requerida.";

            if (NormalizeName(name).Length > NameMaxLength)
                return $"La razón social no puede exceder los {NameMaxLength} caracteres.";

            return null;
        }

        /// <summary>Qué impide cambiar el RUC por ese, o null si se puede (o si no cambia).</summary>
        public string? RucChangeError(string ruc, bool hasPurchases) =>
            hasPurchases && NormalizeRuc(ruc) != Ruc
                ? $"{Name} ya tiene compras registradas con el RUC {Ruc}, así que el RUC no se puede cambiar. Si es otra empresa, regístrala como una empresa nueva."
                : null;

        private static string ValidateRuc(string ruc)
        {
            DomainException.ThrowIf(RucError(ruc));
            return NormalizeRuc(ruc);
        }

        private static string ValidateName(string name)
        {
            DomainException.ThrowIf(NameError(name));
            return NormalizeName(name);
        }
    }
}
