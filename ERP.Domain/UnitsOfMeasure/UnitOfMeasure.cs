using System.Globalization;
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

        /// <summary>
        /// Qué impide usar la unidad en un producto o una compra, o null si se puede: que no exista en el catálogo o
        /// que la empresa no la use (desactivada).
        /// </summary>
        /// <param name="unit">La unidad encontrada con ese código, o null si no existe.</param>
        public static string? UsableError(UnitOfMeasure? unit, string code) =>
            unit is null
                ? $"La unidad de medida '{code.Trim()}' no existe en el catálogo de SUNAT."
                : !unit.IsActive
                    ? $"La unidad de medida '{unit.Name}' está desactivada. Actívala en Administración › Unidades de medida."
                    : null;

        /// <summary>El nombre corto tal como se guarda: sin espacios al inicio ni al final, ni dobles en medio.</summary>
        public static string NormalizeName(string name) =>
            TextNormalizer.CollapseSpaces(name);

        /// <param name="units">Todo el catálogo, para que el nombre no se confunda con el de otra unidad.</param>
        public void UpdateName(string name, IReadOnlyCollection<UnitOfMeasure> units)
        {
            if (NameError(name, Id, units) is { } error)
                throw new DomainException(error);

            Name = NormalizeName(name);
        }

        /// <summary>
        /// Qué tiene de malo el nombre corto, o null si está bien. No puede ser el nombre corto, el nombre SUNAT ni el
        /// código de otra unidad (sin distinguir mayúsculas ni tildes): la planilla de Excel reconoce la unidad por
        /// cualquiera de ellos, y con dos iguales no sabría cuál es.
        /// </summary>
        /// <param name="unitId">La unidad que se renombra.</param>
        /// <param name="units">Todo el catálogo.</param>
        public static string? NameError(string? name, Guid unitId, IReadOnlyCollection<UnitOfMeasure> units)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "El nombre es requerido.";

            var normalized = NormalizeName(name);
            if (normalized.Length > NameMaxLength)
                return $"El nombre no puede exceder los {NameMaxLength} caracteres.";

            if (units.FirstOrDefault(u => u.Id != unitId && u.IsKnownAs(normalized)) is { } other)
                return $"«{normalized}» ya identifica a la unidad {other.Name} ({other.Code}). Elige otro nombre.";

            return null;
        }

        /// <summary>
        /// Si el texto es el nombre corto, el nombre SUNAT o el código de esta unidad, sin distinguir mayúsculas ni
        /// tildes ("docena", "DOCENA", "dzn"). Así se reconoce la unidad escrita en la planilla de Excel.
        /// </summary>
        public bool IsKnownAs(string text)
        {
            var trimmed = text.Trim();
            return SameText(trimmed, Name) || SameText(trimmed, SunatName) || SameText(trimmed, Code);
        }

        public void Activate() =>
            IsActive = true;

        /// <param name="productsUsing">Cuántos productos (activos o no) la tienen como unidad.</param>
        public void Deactivate(int productsUsing)
        {
            if (DeactivateError(productsUsing) is { } error)
                throw new DomainException(error);

            IsActive = false;
        }

        /// <summary>
        /// Qué impide desactivarla, o null si se puede. Una unidad que usa algún producto no se desactiva: el producto
        /// quedaría con una unidad que ya no aparece en las listas. Primero hay que cambiarles la unidad.
        /// </summary>
        public string? DeactivateError(int productsUsing) =>
            productsUsing switch
            {
                < 0 => throw new DomainException("La cantidad de productos no puede ser negativa."),
                0 => null,
                1 => $"No se puede desactivar {Name}: la usa 1 producto. Cámbiale la unidad de medida y vuelve a intentarlo.",
                _ => $"No se puede desactivar {Name}: la usan {productsUsing.ToString(CultureInfo.InvariantCulture)} productos. Cámbiales la unidad de medida y vuelve a intentarlo."
            };

        private static bool SameText(string a, string b) =>
            string.Compare(a, b, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
    }
}
