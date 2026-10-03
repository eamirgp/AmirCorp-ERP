namespace ERP.Application.Features.Auth.Login
{
    /// <summary>Lo que recibe la pantalla al iniciar o renovar la sesión. El refresh token no viaja aquí: va en la cookie.</summary>
    /// <param name="Token">Token de acceso corto (15 minutos); la pantalla lo guarda solo en memoria.</param>
    /// <param name="SessionExpiresAt">Cuándo vence la sesión si no se usa (UTC).</param>
    public sealed record LoginResponseDto(
        string Token,
        DateTime SessionExpiresAt
        );
}
