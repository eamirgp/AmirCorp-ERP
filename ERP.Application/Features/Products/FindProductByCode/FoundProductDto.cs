namespace ERP.Application.Features.Products.FindProductByCode
{
    /// <summary>El producto que ya usa ese código interno, con el aviso listo para mostrar.</summary>
    /// <param name="Message">"El código interno X ya es de «Y» (desactivado). Usa otro código interno."</param>
    public sealed record FoundProductDto(Guid Id, string Code, string Name, bool IsActive, string Message);
}
