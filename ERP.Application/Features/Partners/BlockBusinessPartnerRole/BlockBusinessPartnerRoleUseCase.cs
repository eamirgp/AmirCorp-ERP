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
