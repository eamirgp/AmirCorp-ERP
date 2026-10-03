using ERP.Api.Extensions;
using ERP.Application.Features.UnitsOfMeasure.ListAllUnitsOfMeasure;
using ERP.Application.Features.UnitsOfMeasure.ToggleUnitOfMeasure;
using ERP.Application.Features.UnitsOfMeasure.UpdateUnitOfMeasure;
using ERP.Domain.UnitsOfMeasure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.UnitsOfMeasure
{
    /// <summary>
    /// Catálogo de unidades de medida de SUNAT: la empresa elige cuáles usa y les puede dar un nombre corto.
    /// Las unidades no se crean ni se borran aquí; vienen del catálogo N.° 03 y se cargan con las migraciones.
    /// Para elegir una unidad en un formulario se usa GET api/catalogs/units-of-measure (solo las activas).
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    [Route("api/units-of-measure")]
    public sealed class UnitOfMeasureController : ControllerBase
    {
        private readonly IListAllUnitsOfMeasureUseCase _listAllUseCase;
        private readonly IUpdateUnitOfMeasureUseCase _updateUseCase;
        private readonly IActivateUnitOfMeasureUseCase _activateUseCase;
        private readonly IDeactivateUnitOfMeasureUseCase _deactivateUseCase;

        public UnitOfMeasureController(
            IListAllUnitsOfMeasureUseCase listAllUseCase,
            IUpdateUnitOfMeasureUseCase updateUseCase,
            IActivateUnitOfMeasureUseCase activateUseCase,
            IDeactivateUnitOfMeasureUseCase deactivateUseCase
            )
        {
            _listAllUseCase = listAllUseCase;
            _updateUseCase = updateUseCase;
            _activateUseCase = activateUseCase;
            _deactivateUseCase = deactivateUseCase;
        }

        /// <summary>Todo el catálogo: primero las activas y luego las demás, cada grupo por nombre de la A a la Z.</summary>
        [HttpGet]
        [ProducesResponseType<IReadOnlyCollection<UnitOfMeasureListItemDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List() =>
            Ok(await _listAllUseCase.ExecuteAsync());

        /// <summary>Cambia el nombre corto. El código y el nombre de SUNAT no se cambian.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitOfMeasureRequest request)
        {
            var errors = request.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _updateUseCase.ExecuteAsync(new UpdateUnitOfMeasureDto(id, request.Name!));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _activateUseCase.ExecuteAsync(id);
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        /// <summary>No se puede desactivar una unidad que usa algún producto (409 con el motivo).</summary>
        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _deactivateUseCase.ExecuteAsync(id);
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }
    }

    /// <summary>
    /// Aviso previo: lo que se puede revisar sin el catálogo. Que el nombre no sea el de otra unidad lo revisa el caso de
    /// uso con <see cref="UnitOfMeasure.NameError"/>, la regla del dominio.
    /// </summary>
    public sealed record UpdateUnitOfMeasureRequest(string? Name)
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("El nombre es requerido.");
            else if (UnitOfMeasure.NormalizeName(Name).Length > UnitOfMeasure.NameMaxLength)
                errors.Add($"El nombre no puede exceder los {UnitOfMeasure.NameMaxLength} caracteres.");

            return errors;
        }
    }
}
