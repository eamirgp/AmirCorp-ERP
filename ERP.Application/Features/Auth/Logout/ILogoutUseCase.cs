namespace ERP.Application.Features.Auth.Logout
{
    public interface ILogoutUseCase
    {
        /// <param name="refreshToken">El token de la cookie del navegador, o null si no hay.</param>
        Task ExecuteAsync(string? refreshToken);
    }
}
