using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Partners.Enums;

namespace ERP.Domain.Partners
{
    public sealed class BusinessPartner : AuditableEntity
    {
        public const int NameMaxLength = 100;

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
            ValidateIdentityDocument(identityDocumentType, documentNumber, country);
            ValidateName(name);
            ValidateRoles(isClient, isSupplier, identityDocumentType);

            return new(Guid.CreateVersion7(), identityDocumentType, documentNumber, country, name, isClient, isSupplier, isActive: true);
        }

        public void Update(IdentityDocumentType identityDocumentType, string documentNumber, Country country, string name, bool isClient, bool isSupplier)
        {
            ValidateIdentityDocument(identityDocumentType, documentNumber, country);
            ValidateName(name);
            ValidateRoles(isClient, isSupplier, identityDocumentType);

            IdentityDocumentType = identityDocumentType;
            DocumentNumber = documentNumber;
            Country = country;
            Name = name;
            IsClient = isClient;
            IsSupplier = isSupplier;
        }

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        private static void ValidateIdentityDocument(IdentityDocumentType identityDocumentType, string documentNumber, Country country)
        {
            if (!Enum.IsDefined(identityDocumentType))
                throw new DomainException("El tipo de documento es inválido.");

            if (string.IsNullOrWhiteSpace(documentNumber))
                throw new DomainException("El número de documento es requerido.");

            if (!identityDocumentType.IsValidDocumentNumber(documentNumber))
                throw new DomainException("El número de documento es inválido.");

            if (!Enum.IsDefined(country))
                throw new DomainException("El país es inválido.");

            if (identityDocumentType.RequiresPeruvianCountry && !country.IsPeru)
                throw new DomainException("Para DNI o RUC el país debe ser Perú.");

            if (!identityDocumentType.RequiresPeruvianCountry && country.IsPeru)
                throw new DomainException("Para documento tributario extranjero corresponde a un socio no domiciliado.");
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre es requerido.");

            if (name.Length > NameMaxLength)
                throw new DomainException($"El nombre no puede exceder los {NameMaxLength} caracteres.");
        }

        private static void ValidateRoles(bool isClient, bool isSupplier, IdentityDocumentType identityDocumentType)
        {
            if (!isClient && !isSupplier)
                throw new DomainException("El socio debe ser cliente, proveedor o ambos.");

            if (isSupplier && !identityDocumentType.CanIssueTaxDocuments)
                throw new DomainException("Un proveedor debe tener RUC o ser tributario extranjero para emitir comprobantes válidos.");
        }
    }
}
