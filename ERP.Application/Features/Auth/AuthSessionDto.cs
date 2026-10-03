namespace ERP.Application.Features.Auth
{
    /// <summary>
    /// Lo que entrega iniciar sesión o renovarla. El token de acceso va a la pantalla; el refresh token, a una cookie que
    /// JavaScript no puede leer (lo pone la API). Si no hay refresh token nuevo, la cookie que ya tiene el navegador sigue.
    /// </summary>
    /// <param name="AccessToken">Token corto (15 minutos) para los pedidos.</param>
    /// <param name="RefreshToken">Token nuevo para la cookie, o null si no cambia.</param>
    /// <param name="SessionExpiresAt">Cuándo vence la sesión si no se usa (UTC): la pantalla cierra sesión a esa hora.</param>
    public sealed record AuthSessionDto(string AccessToken, string? RefreshToken, DateTime SessionExpiresAt);
}
