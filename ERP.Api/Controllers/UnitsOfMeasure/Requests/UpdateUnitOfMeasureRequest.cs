using ERP.Application.Features.UnitsOfMeasure.UpdateUnitOfMeasure;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Api.Controllers.UnitsOfMeasure.Requests
{
    /// <summary>
    /// Aviso previo: lo que se puede revisar sin el catálogo (<see cref="UnitOfMeasure.NameFormatError"/>). Que el nombre
    /// no sea el de otra unidad lo revisa el caso de uso con <see cref="UnitOfMeasure.NameError"/>, la regla completa.
    /// </summary>
    /// <param name="Name">El nombre corto que se ve en la pantalla (ej.: "Caja").</param>
    /// <param name="RowVersion">La versión que se vio en la lista: si otra persona la cambió mientras tanto, responde 409.</param>
    public sealed record UpdateUnitOfMeasureRequest(string? Name, uint? RowVersion)
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (RowVersion is null)
                errors.Add("Falta la versión de la unidad. Vuelve a abrir el formulario.");

            if (UnitOfMeasure.NameFormatError(Name) is { } nameError)
                errors.Add(nameError);

            return errors;
        }

        public UpdateUnitOfMeasureDto ToDto(Guid id) =>
            new(id, Name!, RowVersion!.Value);
    }
}
