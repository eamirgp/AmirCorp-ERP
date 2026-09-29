using ERP.Domain.Common;
using ERP.Domain.SavedViews.Enums;

namespace ERP.Domain.SavedViews
{
    /// <summary>
    /// Filtros, orden y filas por página que un usuario guarda con un nombre para una pantalla de lista.
    /// Es una preferencia personal: cada usuario ve solo las suyas. Una por pantalla puede ser la predeterminada.
    /// </summary>
    public sealed class SavedView : AuditableEntity
    {
        public const int NameMaxLength = 50;
        public const int FiltersMaxLength = 2000;
        public const int MaxPerScreen = 20;

        public Guid UserId { get; }
        public SavedViewScreen Screen { get; }
        public string Name { get; private set; }
        /// <summary>Filtros tal como los guarda la pantalla. La API no los interpreta.</summary>
        public string Filters { get; private set; }
        public bool IsDefault { get; private set; }

        private SavedView(Guid id, Guid userId, SavedViewScreen screen, string name, string filters, bool isDefault) : base(id)
        {
            UserId = userId;
            Screen = screen;
            Name = name;
            Filters = filters;
            IsDefault = isDefault;
        }

        public static SavedView Create(Guid userId, SavedViewScreen screen, string name, string filters, bool isDefault)
        {
            if (userId == Guid.Empty)
                throw new DomainException("El usuario es requerido.");

            if (!Enum.IsDefined(screen))
                throw new DomainException("La pantalla es inválida.");

            return new(Guid.CreateVersion7(), userId, screen, ValidateName(name), ValidateFilters(filters), isDefault);
        }

        public static string NormalizeName(string name) =>
            name.Trim();

        public void Rename(string name) =>
            Name = ValidateName(name);

        public void UpdateFilters(string filters) =>
            Filters = ValidateFilters(filters);

        public void MarkAsDefault() =>
            IsDefault = true;

        public void UnmarkAsDefault() =>
            IsDefault = false;

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre de la vista es requerido.");

            var normalized = NormalizeName(name);

            if (normalized.Length > NameMaxLength)
                throw new DomainException($"El nombre de la vista no puede exceder los {NameMaxLength} caracteres.");

            return normalized;
        }

        private static string ValidateFilters(string filters)
        {
            if (filters is null)
                throw new DomainException("Los filtros de la vista son requeridos.");

            if (filters.Length > FiltersMaxLength)
                throw new DomainException($"Los filtros de la vista no pueden exceder los {FiltersMaxLength} caracteres.");

            return filters;
        }
    }
}
