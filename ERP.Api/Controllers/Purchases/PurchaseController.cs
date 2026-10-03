using ERP.Application.Common.Pagination;
using ERP.Application.Common.Responses;
using ERP.Api.Controllers.Purchases.Requests;
using ERP.Api.Extensions;
using ERP.Application.Features.Purchases.CancelPurchase;
using ERP.Application.Features.Purchases.CreatePurchase;
using ERP.Application.Features.Purchases.GetPurchase;
using ERP.Application.Features.Purchases.ListPurchases;
using ERP.Application.Features.Purchases.PreviewPurchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Purchases
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    [Route("api/purchases")]
    public sealed class PurchaseController : ControllerBase
    {
        private readonly ICreatePurchaseUseCase _createPurchaseUseCase;
        private readonly IListPurchasesUseCase _listPurchasesUseCase;
        private readonly IGetPurchaseUseCase _getPurchaseUseCase;
        private readonly ICancelPurchaseUseCase _cancelPurchaseUseCase;
        private readonly IPreviewPurchaseUseCase _previewPurchaseUseCase;

        public PurchaseController(
            ICreatePurchaseUseCase createPurchaseUseCase,
            IListPurchasesUseCase listPurchasesUseCase,
            IGetPurchaseUseCase getPurchaseUseCase,
            ICancelPurchaseUseCase cancelPurchaseUseCase,
            IPreviewPurchaseUseCase previewPurchaseUseCase
            )
        {
            _createPurchaseUseCase = createPurchaseUseCase;
            _listPurchasesUseCase = listPurchasesUseCase;
            _getPurchaseUseCase = getPurchaseUseCase;
            _cancelPurchaseUseCase = cancelPurchaseUseCase;
            _previewPurchaseUseCase = previewPurchaseUseCase;
        }

        /// <summary>
        /// Calcula los montos y totales de una compra mientras se llena, sin guardarla.
        /// </summary>
        [HttpPost("preview")]
        [ProducesResponseType<PreviewPurchaseResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Preview([FromBody] PreviewPurchaseRequest previewPurchaseRequest) =>
            Ok(await _previewPurchaseUseCase.ExecuteAsync(previewPurchaseRequest.ToDto()));

        [HttpPost]
        [ProducesResponseType<CreatedResponseDto>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseRequest createPurchaseRequest)
        {
            var errors = createPurchaseRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _createPurchaseUseCase.ExecuteAsync(createPurchaseRequest.ToDto());
            return result.ToActionResult(StatusCodes.Status201Created);
        }

        [HttpGet]
        [ProducesResponseType<SortedPagedResult<ListPurchasesResponseDto, PurchaseSortBy>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] ListPurchasesRequest listPurchasesRequest) =>
            Ok(await _listPurchasesUseCase.ExecuteAsync(listPurchasesRequest.ToDto()));

        [HttpGet("{id:guid}")]
        [ProducesResponseType<GetPurchaseResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _getPurchaseUseCase.ExecuteAsync(new GetPurchaseDto(id));
            return result.ToActionResult(StatusCodes.Status200OK);
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody]CancelPurchaseRequest cancelPurchaseRequest)
        {
            var errors = cancelPurchaseRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _cancelPurchaseUseCase.ExecuteAsync(cancelPurchaseRequest.ToDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }
    }
}
