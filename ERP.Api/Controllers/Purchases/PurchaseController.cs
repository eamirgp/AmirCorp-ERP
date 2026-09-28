using ERP.Application.Common.Pagination;
using ERP.Application.Common.Responses;
using ERP.Api.Controllers.Purchases.Requests;
using ERP.Api.Extensions;
using ERP.Application.Features.Purchases.CancelPurchase;
using ERP.Application.Features.Purchases.CreatePurchase;
using ERP.Application.Features.Purchases.GetPurchase;
using ERP.Application.Features.Purchases.ListPurchases;
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

        public PurchaseController(
            ICreatePurchaseUseCase createPurchaseUseCase,
            IListPurchasesUseCase listPurchasesUseCase,
            IGetPurchaseUseCase getPurchaseUseCase,
            ICancelPurchaseUseCase cancelPurchaseUseCase
            )
        {
            _createPurchaseUseCase = createPurchaseUseCase;
            _listPurchasesUseCase = listPurchasesUseCase;
            _getPurchaseUseCase = getPurchaseUseCase;
            _cancelPurchaseUseCase = cancelPurchaseUseCase;
        }

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
        [ProducesResponseType<PagedResult<ListPurchasesResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] ListPurchasesRequest listPurchasesRequest) =>
            Ok(await _listPurchasesUseCase.ExecuteAsync(listPurchasesRequest.ToDto()));

        [HttpGet("{id:guid}")]
        [ProducesResponseType<GetPurchaseResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await _getPurchaseUseCase.ExecuteAsync(new GetPurchaseDto(id));
            return response is null ? NotFound() : Ok(response);
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
