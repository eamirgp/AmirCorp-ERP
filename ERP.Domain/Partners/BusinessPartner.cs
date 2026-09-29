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
        public Country Country { get; private set; }
        public string Name { get; private set; }
        public bool IsClient { get; private set; }
        public bool IsSupplier { get; private set; }
        public bool IsActive { get; private set; }

        private BusinessPartner(Guid id, IdentityDocumentType identityDocumentType, string documentNumber, Country country, string name, bool isClient, bool isSupplier, bool isActive) : base(id)
        {
            IdentityDocumentType = identityDocumentType;
            DocumentNumber = documentNumber;
            Country = country;
            Name = name;
            IsClient = isClient;
            IsSupplier = isSupplier;
            IsActive = isActive;
        }

        public static BusinessPartner Create(IdentityDocumentType identityDocumentType, string documentNumber, Country country, string name, bool isClient, bool isSupplier)
        {
            var normalizedDocument = ValidateIdentityDocument(identityDocumentType, documentNumber, country);
            var normalizedName = ValidateName(name);
            ValidateRoles(isClient, isSupplier, identityDocumentType);

            return new(Guid.CreateVersion7(), identityDocumentType, normalizedDocument, country, normalizedName, isClient, isSupplier, isActive: true);
        }

        public void Update(IdentityDocumentType identityDocumentType, string documentNumber, Country country, string name, bool isClient, bool isSupplier)
        {
            var normalizedDocument = ValidateIdentityDocument(identityDocumentType, documentNumber, country);
            var normalizedName = ValidateName(name);
            ValidateRoles(isClient, isSupplier, identityDocumentType);

            IdentityDocumentType = identityDocumentType;
            DocumentNumber = normalizedDocument;
            Country = country;
            Name = normalizedName;
            IsClient = isClient;
            IsSupplier = isSupplier;
        }

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        /// <summary>El nombre tal como se guarda: sin espacios al inicio ni al final, ni dobles en medio.</summary>
        public static string NormalizeName(string name) =>
            Spaces.Replace(name.Trim(), " ");

        /// <summary>País que corresponde al documento: DNI y RUC son siempre de Perú; el extranjero, el que se indique.</summary>
        public static Country? CountryFor(IdentityDocumentType identityDocumentType, Country? country) =>
            identityDocumentType.RequiresPeruvianCountry ? Country.PE : country;

        private static string ValidateIdentityDocument(IdentityDocumentType identityDocumentType, string documentNumber, Country country)
        {
            if (!Enum.IsDefined(identityDocumentType))
                throw new DomainException("El tipo de documento es inválido.");

            var normalized = IdentityDocumentTypeExtensions.NormalizeDocumentNumber(documentNumber ?? "");
            if (identityDocumentType.DocumentNumberError(normalized) is { } error)
                throw new DomainException(error);

            if (!Enum.IsDefined(country))
                throw new DomainException("El país es inválido.");

            if (identityDocumentType.RequiresPeruvianCountry && !country.IsPeru)
                throw new DomainException("Para DNI o RUC el país debe ser Perú.");

            if (!identityDocumentType.RequiresPeruvianCountry && country.IsPeru)
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
        }
    }
}
