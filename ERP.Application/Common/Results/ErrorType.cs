namespace ERP.Application.Common.Results
{
    public enum ErrorType
    {
        BadRequest,
        Unauthorized,
        Forbidden,
        NotFound,
        Conflict,
        /// <summary>Un servicio externo (por ejemplo, la consulta de RUC) no respondió: 503.</summary>
        Unavailable,
        /// <summary>Demasiados intentos seguidos (por ejemplo, contraseñas equivocadas): 429.</summary>
        TooManyRequests
    }
}
