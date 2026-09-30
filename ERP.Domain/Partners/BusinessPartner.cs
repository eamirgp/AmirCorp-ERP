using System.Text.RegularExpressions;
using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Partners.Enums;

namespace ERP.Domain.Partners
{
    public sealed class BusinessPartner : AuditableEntity
    {
        public const int NameMaxLength = 100;

        private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);

        public IdentityDocumentType IdentityDocumentType { get; private set; }
        public string DocumentNumber { get; private set; }
        /// <summary>Código ISO del país (catálogo N.° 04 de SUNAT): PE con DNI o RUC; otro con documento extranjero.</summary>
        public string CountryCode { get; private set; }
        public string Name { get; private set; }
        public bool IsClient { get; private set; }
        public bool IsSupplier { get; private set; }
        public bool IsActive { get; private set; }

        private BusinessPartner(Guid id, IdentityDocumentType identityDocumentType, string documentNumber, string countryCode, string name, bool isClient, bool isSupplier, bool isActive) : base(id)
        {
            IdentityDocumentType = identityDocumentType;
            DocumentNumber = documentNumber;
            CountryCode = countryCode;
            Name = name;
            IsClient = isClient;
            IsSupplier = isSupplier;
            IsActive = isActive;
        }

        public static BusinessPartner Create(IdentityDocumentType identityDocumentType, string documentNumber, string countryCode, string name, bool isClient, bool isSupplier)
        {
            var normalizedDocument = ValidateIdentityDocument(identityDocumentType, documentNumber);
            var normalizedCountry = ValidateCountry(identityDocumentType, countryCode);
            var normalizedName = ValidateName(name);
            ValidateRoles(isClient, isSupplier, identityDocumentType);

            return new(Guid.CreateVersion7(), identityDocumentType, normalizedDocument, normalizedCountry, normalizedName, isClient, isSupplier, isActive: true);
        }

        /// <summary>
        /// Corrige los datos. Los roles no cambian aquí (se agregan con <see cref="AddClientRole"/> y
        /// <see cref="AddSupplierRole"/>), pero el documento nuevo debe servir para los roles que ya tiene.
        /// </summary>
        public void Update(IdentityDocumentType identityDocumentType, string documentNumber, string countryCode, string name)
        {
            var normalizedDocument = ValidateIdentityDocument(identityDocumentType, documentNumber);
            var normalizedCountry = ValidateCountry(identityDocumentType, countryCode);
            var normalizedName = ValidateName(name);
            ValidateRoles(IsClient, IsSupplier, identityDocumentType);

            IdentityDocumentType = identityDocumentType;
            DocumentNumber = normalizedDocument;
            CountryCode = normalizedCountry;
            Name = normalizedName;
        }

        /// <summary>Un proveedor pasa a ser también cliente (si su documento lo permite).</summary>
        public void AddClientRole()
        {
            ValidateRoles(isClient: true, IsSupplier, IdentityDocumentType);
            IsClient = true;
        }

        /// <summary>Un cliente pasa a ser también proveedor (si su documento lo permite).</summary>
        public void AddSupplierRole()
        {
            ValidateRoles(IsClient, isSupplier: true, IdentityDocumentType);
            IsSupplier = true;
        }

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        /// <summary>El nombre tal como se guarda: sin espacios al inicio ni al final, ni dobles en medio.</summary>
        public static string NormalizeName(string name) =>
            Spaces.Replace(name.Trim(), " ");

        /// <summary>País que corresponde al documento: DNI y RUC son siempre de Perú; el extranjero, el que se indique.</summary>
        public static string? CountryFor(IdentityDocumentType identityDocumentType, string? countryCode) =>
            identityDocumentType.RequiresPeruvianCountry ? Countries.Peru : countryCode;

        private static string ValidateIdentityDocument(IdentityDocumentType identityDocumentType, string documentNumber)
        {
            if (!Enum.IsDefined(identityDocumentType))
                throw new DomainException("El tipo de documento es inválido.");

            var normalized = IdentityDocumentTypeExtensions.NormalizeDocumentNumber(documentNumber ?? "");
            if (identityDocumentType.DocumentNumberError(normalized) is { } error)
                throw new DomainException(error);

            return normalized;
        }

        private static string ValidateCountry(IdentityDocumentType identityDocumentType, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode) || !Countries.Exists(countryCode))
                throw new DomainException("El país es inválido.");

            var normalized = Countries.NormalizeCode(countryCode);

            if (identityDocumentType.RequiresPeruvianCountry && normalized != Countries.Peru)
                throw new DomainException("Para DNI o RUC el país debe ser Perú.");

            if (!identityDocumentType.RequiresPeruvianCountry && normalized == Countries.Peru)
                throw new DomainException("Un documento extranjero no puede ser de Perú. Elige el país del proveedor.");

            return normalized;
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre es requerido.");

            var normalized = NormalizeName(name);
            if (normalized.Length > NameMaxLength)
                throw new DomainException($"El nombre no puede exceder los {NameMaxLength} caracteres.");

            return normalized;
        }

        private static void ValidateRoles(bool isClient, bool isSupplier, IdentityDocumentType identityDocumentType)
        {
            if (!isClient && !isSupplier)
                throw new DomainException("Marca si es cliente, proveedor o ambos.");

            if (isSupplier && !identityDocumentType.CanIssueTaxDocuments)
                throw new DomainException("Un proveedor debe tener RUC o documento extranjero: con DNI no puede emitir facturas.");

            if (isClient && !identityDocumentType.CanBeClient)
                throw new DomainException("Por ahora solo se vende en Perú: un cliente debe tener RUC o DNI.");
        }
    }
}
