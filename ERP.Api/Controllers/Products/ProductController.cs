using ERP.Api.Controllers.Products.Requests;
using ERP.Api.Extensions;
using ERP.Application.Features.Products.ActivateProduct;
using ERP.Application.Features.Products.CreateProduct;
using ERP.Application.Features.Products.DeactivateProduct;
using ERP.Application.Features.Products.GetProduct;
using ERP.Application.Features.Products.ListProducts;
using ERP.Application.Features.Products.UpdateProduct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Products
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    [Route("api/products")]
    public sealed class ProductController : ControllerBase
    {
        private readonly ICreateProductUseCase _createProductUseCase;
        private readonly IListProductsUseCase _listProductsUseCase;
        private readonly IActivateProductUseCase _activateProductUseCase;
        private readonly IDeactivateProductUseCase _deactivateProductUseCase;
        private readonly IUpdateProductUseCase _updateProductUseCase;
        private readonly IGetProductUseCase _getProductUseCase;

        public ProductController(
            ICreateProductUseCase createProductUseCase,
            IListProductsUseCase listProductsUseCase,
            IActivateProductUseCase activateProductUseCase,
            IDeactivateProductUseCase deactivateProductUseCase,
            IUpdateProductUseCase updateProductUseCase,
            IGetProductUseCase getProductUseCase
            )
        {
            _createProductUseCase = createProductUseCase;
            _listProductsUseCase = listProductsUseCase;
            _activateProductUseCase = activateProductUseCase;
            _deactivateProductUseCase = deactivateProductUseCase;
            _updateProductUseCase = updateProductUseCase;
            _getProductUseCase = getProductUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateProductRequest createProductRequest)
        {
            var errors = createProductRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _createProductUseCase.ExecuteAsync(createProductRequest.ToDto());
            return result.ToActionResult(StatusCodes.Status201Created);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery]ListProductsRequest listProductsRequest) =>
            Ok(await _listProductsUseCase.ExecuteAsync(listProductsRequest.ToDto()));

        [HttpPatch("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _activateProductUseCase.ExecuteAsync(new ActivateProductDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _deactivateProductUseCase.ExecuteAsync(new DeactivateProductDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody]UpdateProductRequest updateProductRequest)
        {
            var errors = updateProductRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _updateProductUseCase.ExecuteAsync(updateProductRequest.ToDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await _getProductUseCase.ExecuteAsync(new GetProductDto(id));
            return response is null ? NotFound() : Ok(response);
        }
    }
}
