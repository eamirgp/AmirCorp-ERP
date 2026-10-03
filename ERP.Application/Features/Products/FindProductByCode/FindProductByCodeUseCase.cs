using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Products;

namespace ERP.Application.Features.Products.FindProductByCode
{
    /// <summary>El producto que ya usa ese código interno, con el aviso listo para mostrar.</summary>
    /// <param name="Message">"El código interno X ya es de «Y» (desactivado). Usa otro código interno."</param>
    public sealed record FoundProductDto(Guid Id, string Code, string Name, bool IsActive, string Message);

    public interface IFindProductByCodeUseCase
    {
        /// <returns>El producto que tiene ese código, o 404 si ninguno lo tiene (el código está libre).</returns>
        Task<Result<FoundProductDto>> ExecuteAsync(string code);
    }

    /// <summary>
    /// Quién tiene ya un código interno. Al crear un producto desde la compra, la pantalla lo consulta para avisar en el
    /// momento que el código está ocupado (el registro igual lo rechaza), en vez de enterarse al guardar. El aviso es el
    /// mismo que da el registro (<see cref="Product.CodeTakenError"/>).
    /// </summary>
    internal sealed class FindProductByCodeUseCase : IFindProductByCodeUseCase
    {
        private const string Free = "Ningún producto tiene ese código interno.";

        private readonly IProductRepository _productRepository;

        public FindProductByCodeUseCase(IProductRepository productRepository) => _productRepository = productRepository;

        public async Task<Result<FoundProductDto>> ExecuteAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result<FoundProductDto>.Failure([Free], ErrorType.NotFound);

            var product = await _productRepository.FindByCodeAsync(code);
            return Result<FoundProductDto>.FoundOr(
                product is null ? null : new FoundProductDto(product.Id, product.Code, product.Name, product.IsActive, Product.CodeTakenError(product)),
                Free);
        }
    }
}
