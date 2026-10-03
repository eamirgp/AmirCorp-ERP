using ERP.Domain.Common;
using ERP.Domain.SavedViews.Enums;

namespace ERP.Domain.SavedViews
{
    /// <summary>
    /// Filtros, orden y filas por página que un usuario guarda con un nombre para una pantalla de lista.
    /// Es una preferencia personal: cada usuario ve solo las suyas. Una por pantalla puede ser la predeterminada.
    /// Las reglas entre vistas (nombre que no se repite, máximo por pantalla, una sola predeterminada) reciben las demás
    /// vistas del mismo usuario y pantalla.
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

        /// <param name="siblings">Las vistas que el usuario ya tiene en esa pantalla.</param>
        public static SavedView Create(Guid userId, SavedViewScreen screen, string name, string filters, bool isDefault, IReadOnlyCollection<SavedView> siblings)
        {
            if (userId == Guid.Empty)
                throw new DomainException("El usuario es requerido.");
            DomainException.ThrowIf(ScreenError(screen));
            EnsureSiblings(userId, screen, siblings, exceptId: null);
            DomainException.ThrowIf(NameError(name));
            DomainException.ThrowIf(FiltersError(filters));
            DomainException.ThrowIf(CreateError(name, siblings));

            var view = new SavedView(Guid.CreateVersion7(), userId, screen, NormalizeName(name), filters, isDefault: false);
            if (isDefault)
                view.MakeDefault(siblings);
            return view;
        }

        /// <summary>Renombra, cambia los filtros y marca o desmarca como predeterminada.</summary>
        /// <param name="siblings">Las vistas del usuario en esa pantalla (puede incluir esta).</param>
        public void Update(string name, string filters, bool isDefault, IReadOnlyCollection<SavedView> siblings)
        {
            EnsureSiblings(UserId, Screen, siblings, exceptId: Id);
            DomainException.ThrowIf(NameError(name));
            DomainException.ThrowIf(FiltersError(filters));
            DomainException.ThrowIf(NameTakenError(name, siblings, Id));

            Name = NormalizeName(name);
            Filters = filters;

            if (isDefault)
                MakeDefault(siblings);
            else
                IsDefault = false;
        }

        public static string NormalizeName(string name) =>
            TextNormalizer.CollapseSpaces(name);

        public static string? ScreenError(SavedViewScreen? screen) =>
            screen switch
            {
                null => "La pantalla es requerida.",
                { } value when !Enum.IsDefined(value) => "La pantalla es inválida.",
                _ => null
            };

        public static string? NameError(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "El nombre de la vista es requerido.";

            if (NormalizeName(name).Length > NameMaxLength)
                return $"El nombre de la vista no puede exceder los {NameMaxLength} caracteres.";

            return null;
        }

        /// <summary>Los filtros son el texto que guarda la pantalla: pueden ir vacíos, pero no faltar.</summary>
        public static string? FiltersError(string? filters) =>
            filters switch
            {
                null => "Los filtros de la vista son requeridos.",
                { Length: > FiltersMaxLength } => $"Los filtros de la vista no pueden exceder los {FiltersMaxLength} caracteres.",
                _ => null
            };

        /// <summary>Qué impide guardar una vista nueva con ese nombre: el máximo por pantalla o un nombre repetido.</summary>
        public static string? CreateError(string name, IReadOnlyCollection<SavedView> siblings) =>
            siblings.Count >= MaxPerScreen
                ? $"Ya tienes {MaxPerScreen} vistas guardadas en esta pantalla. Elimina alguna para guardar otra."
                : NameTakenError(name, siblings, exceptId: null);

        /// <summary>El nombre no se repite en la pantalla, sin distinguir mayúsculas.</summary>
        public static string? NameTakenError(string name, IReadOnlyCollection<SavedView> siblings, Guid? exceptId)
        {
            var normalized = NormalizeName(name);
            return siblings.Any(v => v.Id != exceptId && string.Equals(v.Name, normalized, StringComparison.OrdinalIgnoreCase))
                ? $"Ya tienes una vista llamada «{normalized}»."
                : null;
        }

        // Solo una vista por pantalla es la predeterminada: al elegir esta, las demás dejan de serlo.
        private void MakeDefault(IReadOnlyCollection<SavedView> siblings)
        {
            foreach (var view in siblings.Where(v => v.Id != Id && v.IsDefault))
                view.IsDefault = false;

            IsDefault = true;
        }

        private static void EnsureSiblings(Guid userId, SavedViewScreen screen, IReadOnlyCollection<SavedView> siblings, Guid? exceptId)
        {
            if (siblings.Any(v => v.Id != exceptId && (v.UserId != userId || v.Screen != screen)))
                throw new DomainException("Las vistas comparadas deben ser del mismo usuario y pantalla.");
        }
    }
}
