namespace ERP.Application.Features.Catalogs.GetExchangeRate
{
    /// <summary>Tipo de cambio para llenar una compra, con el texto que explica de dónde salió.</summary>
    public sealed record GetExchangeRateResponseDto(
        decimal Rate,
        // Fecha de lo publicado: anterior a la pedida si ese día no hubo publicación.
        DateOnly Date,
        string Source,
        // "Tipo de cambio venta de SUNAT del 02/10/2026."
        string Description
        );
}
