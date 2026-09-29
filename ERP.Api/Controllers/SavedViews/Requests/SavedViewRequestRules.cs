using ERP.Domain.SavedViews;

namespace ERP.Api.Controllers.SavedViews.Requests
{
    /// <summary>Validaciones de nombre y filtros que comparten crear y actualizar una vista.</summary>
    internal static class SavedViewRequestRules
    {
        public static void Validate(string? name, string? filters, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("El nombre de la vista es requerido.");
            else if (name.Trim().Length > SavedView.NameMaxLength)
                errors.Add($"El nombre de la vista no puede exceder los {SavedView.NameMaxLength} caracteres.");

            if (filters is null)
                errors.Add("Los filtros de la vista son requeridos.");
            else if (filters.Length > SavedView.FiltersMaxLength)
                errors.Add($"Los filtros de la vista no pueden exceder los {SavedView.FiltersMaxLength} caracteres.");
        }
    }
}
