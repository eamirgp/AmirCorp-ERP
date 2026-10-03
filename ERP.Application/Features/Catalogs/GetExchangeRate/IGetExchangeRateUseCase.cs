using ERP.Application.Common.Results;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.GetExchangeRate
{
    public interface IGetExchangeRateUseCase
    {
        /// <param name="storedOnly">
        /// Solo lo que ya está guardado, sin consultar al servicio externo: la pantalla lo usa para llenar el campo sola
        /// al elegir la fecha, sin gastar consultas. Si no está guardado responde "no encontrado" y lo pide el usuario.
        /// </param>
        Task<Result<GetExchangeRateResponseDto>> ExecuteAsync(Currency currency, DateOnly date, bool storedOnly = false, CancellationToken ct = default);
    }
}
