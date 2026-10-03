using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Features.Partners.AddBusinessPartnerRole;

namespace ERP.Application.Features.Partners.BlockBusinessPartnerRole
{
    /// <param name="Reason">Motivo opcional al bloquear ("Mercadería defectuosa"); se ignora al desbloquear.</param>
    public sealed record BlockBusinessPartnerRoleDto(Guid Id, BusinessPartnerRole Role, bool Blocked, string? Reason);

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
