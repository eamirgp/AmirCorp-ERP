using ERP.Domain.SavedViews;

namespace ERP.Application.Features.SavedViews
{
    /// <summary>Reglas entre las vistas de un mismo usuario y pantalla.</summary>
    internal static class SavedViewRules
    {
        /// <summary>El nombre no se repite en la pantalla, sin distinguir mayúsculas.</summary>
        public static bool NameTaken(IEnumerable<SavedView> views, string name, Guid? excludeId = null)
        {
            var normalized = SavedView.NormalizeName(name);
            return views.Any(v => v.Id != excludeId && string.Equals(v.Name, normalized, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Solo una vista por pantalla es la predeterminada: al elegir una, las demás dejan de serlo.</summary>
        public static void MakeDefault(IEnumerable<SavedView> views, SavedView chosen)
        {
            foreach (var view in views.Where(v => v.Id != chosen.Id && v.IsDefault))
                view.UnmarkAsDefault();

            chosen.MarkAsDefault();
        }
    }
}
