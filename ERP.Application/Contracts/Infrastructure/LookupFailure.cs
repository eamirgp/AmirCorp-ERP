namespace ERP.Application.Contracts.Infrastructure
{
    /// <summary>Por qué una consulta a un servicio externo (RUC, DNI, tipo de cambio) no trajo los datos.</summary>
    public enum LookupFailure
    {
        /// <summary>El servicio no tiene ese dato (un RUC o DNI inexistente, un día sin publicación).</summary>
        NotFound,
        /// <summary>El servicio rechazó la clave (vencida o mal copiada).</summary>
        Unauthorized,
        /// <summary>El servicio no respondió o falló.</summary>
        Unavailable,
        /// <summary>Se acabaron las consultas del plan (Decolecta da 1000 al mes): reintentar no sirve hasta el próximo mes.</summary>
        QuotaExceeded
    }
}
