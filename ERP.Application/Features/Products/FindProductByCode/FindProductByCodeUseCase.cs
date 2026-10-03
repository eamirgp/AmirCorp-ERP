using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Products.FindProductByCode
{
    /// <summary>El producto que ya usa ese código interno.</summary>
    public sealed record FoundProductDto(Guid Id, string Code, string Name, bool IsActive);

    public interface IFindProductByCodeUseCase
    {
        Task<FoundProductDto?> ExecuteAsync(string code);
    }

    /// <summary>
    /// Quién tiene ya un código interno. Al crear un producto desde la compra, la pantalla lo consulta para avisar en el
    /// momento que el código está ocupado (el registro igual lo rechaza), en vez de enterarse al guardar.
    /// </summary>
    internal sealed class FindProductByCodeUseCase : IFindProductByCodeUseCase
    {
        private readonly IProductRepository _productRepository;

        public FindProductByCodeUseCase(IProductRepository productRepository) => _productRepository = productRepository;

        public async Task<FoundProductDto?> ExecuteAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            var product = (await _productRepository.GetByCodesAsync([code])).FirstOrDefault();
            return product is null ? null : new FoundProductDto(product.Id, product.Code, product.Name, product.IsActive);
        }
    }
}
