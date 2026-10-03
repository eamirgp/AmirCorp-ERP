using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Features.Partners.AddBusinessPartnerRole;

namespace ERP.Application.Features.Partners.BlockBusinessPartnerRole
{
    /// <param name="Reason">Motivo opcional al bloquear ("Mercadería defectuosa"); se ignora al desbloquear.</param>
    /// <param name="RowVersion">Versión que se veía en la lista.</param>
    public sealed record BlockBusinessPartnerRoleDto(Guid Id, BusinessPartnerRole Role, bool Blocked, string? Reason, uint RowVersion);

    public interface IBlockBusinessPartnerRoleUseCase
    {
        Task<Result> ExecuteAsync(BlockBusinessPartnerRoleDto request);
    }

    /// <summary>
    /// Bloquea o desbloquea las compras (rol proveedor) o las ventas (rol cliente) de un registro, como los bloqueos
    /// por rol del Business Partner de SAP. El otro rol no cambia. El historial registra el cambio y el motivo.
    /// </summary>
    internal sealed class BlockBusinessPartnerRoleUseCase : IBlockBusinessPartnerRoleUseCase
    {
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BlockBusinessPartnerRoleUseCase(IBusinessPartnerRepository businessPartnerRepository, IUnitOfWork unitOfWork)
        {
            _businessPartnerRepository = businessPartnerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(BlockBusinessPartnerRoleDto request)
        {
            if (!Enum.IsDefined(request.Role))
                return Result.Failure(["El rol es inválido."], ErrorType.BadRequest);

            var businessPartner = await _businessPartnerRepository.GetByIdAsync(request.Id);
            if (businessPartner is null)
                return Result.Failure(["El cliente o proveedor no existe."], ErrorType.NotFound);

            // Ya está así (doble clic, o otra persona hizo lo mismo): no cambia nada y no es un error, ni siquiera de versión.
            var already = request.Role is BusinessPartnerRole.Supplier
                ? businessPartner.IsSupplier && businessPartner.PurchasingAlready(request.Blocked, request.Reason)
                : businessPartner.IsClient && businessPartner.SalesAlready(request.Blocked, request.Reason);
            if (already)
                return Result.Success();

            // Si alguien lo modificó después de abrir la lista (por ejemplo, ya lo bloqueó con otro motivo), no se pisa su cambio.
            if (_businessPartnerRepository.VersionOf(businessPartner) != request.RowVersion)
                return Result.Failure(
                    ["Otra persona modificó este registro mientras lo veías. Actualiza la lista y vuelve a intentarlo."],
                    ErrorType.Conflict
                    );

            // Las mismas reglas del dominio, revisadas antes para responder con el mensaje en vez de una excepción.
            // Desbloquear algo que no estaba bloqueado no es un error: no cambia nada.
            var error = (request.Role, request.Blocked) switch
            {
                (BusinessPartnerRole.Supplier, true) => businessPartner.BlockPurchasingError(request.Reason),
                (BusinessPartnerRole.Client, true) => businessPartner.BlockSalesError(request.Reason),
                _ => null
            };
            if (error is not null)
                return Result.Failure([error], ErrorType.BadRequest);

            switch (request.Role, request.Blocked)
            {
                case (BusinessPartnerRole.Supplier, true): businessPartner.BlockPurchasing(request.Reason); break;
                case (BusinessPartnerRole.Supplier, false): businessPartner.UnblockPurchasing(); break;
                case (BusinessPartnerRole.Client, true): businessPartner.BlockSales(request.Reason); break;
                default: businessPartner.UnblockSales(); break;
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
