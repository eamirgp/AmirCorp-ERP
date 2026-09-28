using ERP.Domain.Catalogs;
using ERP.Domain.Common;

namespace ERP.Domain.Products
{
    public sealed class Product : AuditableEntity
    {
        public const int CodeMaxLength = 50;
        public const int NameMaxLength = 100;

        public string Code { get; private set; }
        public string Name { get; private set; }
        public UnitOfMeasure UnitOfMeasure { get; private set; }
        public IgvAffectation IgvAffectation { get; private set; }
        public decimal SalePrice { get; private set; }
        public bool IsActive { get; private set; }

        private Product(Guid id, string code, string name, UnitOfMeasure unitOfMeasure, IgvAffectation igvAffectation, decimal salePrice, bool isActive) : base(id)
        {
            Code = code;
            Name = name;
            UnitOfMeasure = unitOfMeasure;
            IgvAffectation = igvAffectation;
            SalePrice = salePrice;
            IsActive = isActive;
        }

        public static Product Create(string code, string name, UnitOfMeasure unitOfMeasure, IgvAffectation igvAffectation, decimal salePrice) =>
            new(Guid.CreateVersion7(), ValidateCode(code), ValidateName(name), ValidateUnitOfMeasure(unitOfMeasure), ValidateIgvAffectation(igvAffectation), ValidateSalePrice(salePrice), isActive: true);

        public static string NormalizeCode(string code) =>
            code.ToUpperInvariant();

        public void UpdateCode(string code) =>
            Code = ValidateCode(code);

        public void UpdateName(string name) =>
            Name = ValidateName(name);

        public void UpdateUnitOfMeasure(UnitOfMeasure unitOfMeasure) =>
            UnitOfMeasure = ValidateUnitOfMeasure(unitOfMeasure);

        public void UpdateIgvAffectation(IgvAffectation igvAffectation) =>
            IgvAffectation = ValidateIgvAffectation(igvAffectation);

        public void UpdateSalePrice(decimal salePrice) =>
            SalePrice = ValidateSalePrice(salePrice);

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        private static string ValidateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("El código es requerido.");

            if (code.Length > CodeMaxLength)
                throw new DomainException($"El código no puede exceder los {CodeMaxLength} caracteres.");

            return NormalizeCode(code);
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre es requerido.");

            if (name.Length > NameMaxLength)
                throw new DomainException($"El nombre no puede exceder los {NameMaxLength} caracteres.");

            return name;
        }

        private static UnitOfMeasure ValidateUnitOfMeasure(UnitOfMeasure unitOfMeasure)
        {
            if (!Enum.IsDefined(unitOfMeasure))
                throw new DomainException("La unidad de medida es inválida.");

            return unitOfMeasure;
        }

        private static IgvAffectation ValidateIgvAffectation(IgvAffectation igvAffectation)
        {
            if (!Enum.IsDefined(igvAffectation))
                throw new DomainException("El tipo de afectación del IGV es inválido.");

            return igvAffectation;
        }

        private static decimal ValidateSalePrice(decimal salePrice)
        {
            if (salePrice < 0)
                throw new DomainException("El precio de venta no puede ser negativo.");

            return salePrice;
        }
    }
}
