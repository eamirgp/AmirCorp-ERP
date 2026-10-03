namespace ERP.Application.Contracts.Infrastructure
{
    /// <summary>Crea los refresh tokens (aleatorios, imposibles de adivinar) y calcula la huella que se guarda en la base.</summary>
    public interface IRefreshTokenGenerator
    {
        /// <returns>El token para la cookie del navegador y su huella para la base.</returns>
        (string Token, string Hash) Generate();

        string Hash(string token);
    }
}
