namespace ERP.Application.Features.Auth.Login
{
    public sealed record LoginDto(
        string Email,
        string Password,
        // Dirección del equipo que intenta entrar, para contar los intentos fallidos; null si no se conoce.
        string? ClientIp
        );
}
