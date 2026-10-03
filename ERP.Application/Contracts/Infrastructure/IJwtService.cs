namespace ERP.Application.Contracts.Infrastructure
{
    public interface IJwtService
    {
        /// <summary>
        /// Nombre del dato del token que dice a qué sesión pertenece. No es "sid": ASP.NET lo renombra al leer el token.
        /// </summary>
        const string SessionClaim = "erp_session";

        /// <param name="sessionId">
        /// La sesión del navegador (<c>RefreshToken.FamilyId</c>): en cada pedido se revisa que siga abierta, así cerrar
        /// sesión o restablecer la contraseña deja de servir al momento, sin esperar a que venza el token.
        /// </param>
        string GenerateToken(Guid userId, string name, string email, string role, Guid sessionId);
    }
}
