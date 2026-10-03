using ERP.Domain.SavedViews;

namespace ERP.Api.Controllers.SavedViews.Requests
{
    /// <summary>Validaciones de nombre y filtros que comparten crear y actualizar una vista: las reglas del dominio.</summary>
    internal static class SavedViewRequestRules
    {
        public static void Validate(string? name, string? filters, List<string> errors)
        {
            if (SavedView.NameError(name) is { } nameError)
                errors.Add(nameError);

            if (SavedView.FiltersError(filters) is { } filtersError)
                errors.Add(filtersError);
        }
    }
}
