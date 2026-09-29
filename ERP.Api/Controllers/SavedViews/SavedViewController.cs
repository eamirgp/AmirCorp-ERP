using ERP.Api.Controllers.SavedViews.Requests;
using ERP.Api.Extensions;
using ERP.Application.Common.Responses;
using ERP.Application.Features.SavedViews.CreateSavedView;
using ERP.Application.Features.SavedViews.DeleteSavedView;
using ERP.Application.Features.SavedViews.ListSavedViews;
using ERP.Application.Features.SavedViews.UpdateSavedView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.SavedViews
{
    /// <summary>Vistas guardadas del usuario que inicia sesión. Cada usuario ve y modifica solo las suyas.</summary>
    [Authorize]
    [ApiController]
    [Route("api/saved-views")]
    public sealed class SavedViewController : ControllerBase
    {
        private readonly IListSavedViewsUseCase _listSavedViewsUseCase;
        private readonly ICreateSavedViewUseCase _createSavedViewUseCase;
        private readonly IUpdateSavedViewUseCase _updateSavedViewUseCase;
        private readonly IDeleteSavedViewUseCase _deleteSavedViewUseCase;

        public SavedViewController(
            IListSavedViewsUseCase listSavedViewsUseCase,
            ICreateSavedViewUseCase createSavedViewUseCase,
            IUpdateSavedViewUseCase updateSavedViewUseCase,
            IDeleteSavedViewUseCase deleteSavedViewUseCase
            )
        {
            _listSavedViewsUseCase = listSavedViewsUseCase;
            _createSavedViewUseCase = createSavedViewUseCase;
            _updateSavedViewUseCase = updateSavedViewUseCase;
            _deleteSavedViewUseCase = deleteSavedViewUseCase;
        }

        [HttpGet]
        [ProducesResponseType<IReadOnlyCollection<ListSavedViewsResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery]ListSavedViewsRequest listSavedViewsRequest)
        {
            var errors = listSavedViewsRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            return Ok(await _listSavedViewsUseCase.ExecuteAsync(listSavedViewsRequest.ToDto()));
        }

        [HttpPost]
        [ProducesResponseType<CreatedResponseDto>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody]CreateSavedViewRequest createSavedViewRequest)
        {
            var errors = createSavedViewRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _createSavedViewUseCase.ExecuteAsync(createSavedViewRequest.ToDto());
            return result.ToActionResult(StatusCodes.Status201Created);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody]UpdateSavedViewRequest updateSavedViewRequest)
        {
            var errors = updateSavedViewRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _updateSavedViewUseCase.ExecuteAsync(updateSavedViewRequest.ToDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _deleteSavedViewUseCase.ExecuteAsync(new DeleteSavedViewDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }
    }
}
