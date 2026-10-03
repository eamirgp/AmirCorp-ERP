using ERP.Domain.Common;

namespace ERP.Domain.UnitsOfMeasure
{
    /// <summary>
    /// Unidad de medida del catálogo N.° 03 de SUNAT (UN/ECE Recommendation 20). El código y el nombre oficial vienen
    /// de SUNAT y no se cambian; la empresa solo elige cuáles usa (activas) y puede darles un nombre corto para las
    /// pantallas. Las unidades se cargan con las migraciones: no se crean desde el sistema, porque SUNAT rechaza la
    /// factura con un código que no está en su catálogo.
    /// </summary>
    public sealed class UnitOfMeasure : AuditableEntity
    {
        public const int CodeMaxLength = 3;
        public const int NameMaxLength = 40;
        public const int SunatNameMaxLength = 60;

        /// <summary>
        /// Unidad en que se cuenta el stock: "Unidad" (NIU). Se compra y se vende por caja, docena o suelto, y todo se
        /// convierte a unidades.
        /// </summary>
        public const string BaseUnitCode = "NIU";

        /// <summary>Código SUNAT (NIU, DZN, BX…). Es el que va en la factura electrónica.</summary>
        public string Code { get; }
        /// <summary>Nombre oficial de SUNAT ("UNIDAD (BIENES)").</summary>
        public string SunatName { get; }
        /// <summary>Nombre corto que se muestra en el sistema ("Unidad").</summary>
        public string Name { get; private set; }
        /// <summary>
        /// Cuántas unidades trae siempre (docena = 12, par = 2), o null si cada compra lo indica (una caja trae
        /// lo que diga la factura). En las compras, el factor de conversión debe ser este.
        /// </summary>
        public decimal? FixedConversionFactor { get; }
        public bool IsActive { get; private set; }

        private UnitOfMeasure(Guid id, string code, string sunatName, string name, decimal? fixedConversionFactor, bool isActive) : base(id)
        {
            Code = code;
            SunatName = sunatName;
            Name = name;
            FixedConversionFactor = fixedConversionFactor;
            IsActive = isActive;
        }

        public static string NormalizeCode(string code) =>
            code.Trim().ToUpperInvariant();

        public void UpdateName(string name) =>
            Name = ValidateName(name);

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre es requerido.");

            var normalized = TextNormalizer.CollapseSpaces(name);
            if (normalized.Length > NameMaxLength)
                throw new DomainException($"El nombre no puede exceder los {NameMaxLength} caracteres.");

            return normalized;
        }
    }
}
