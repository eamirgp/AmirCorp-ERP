using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Partners.AddBusinessPartnerRole
{
    /// <summary>
    /// "Registrar también como proveedor / cliente": el mismo registro pasa a estar en las dos listas, como la acción
    /// "Crear como proveedor" de Business Central. Si el documento no sirve para ese rol, el dominio lo rechaza.
    /// </summary>
    internal sealed class AddBusinessPartnerRoleUseCase : IAddBusinessPartnerRoleUseCase
    {
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddBusinessPartnerRoleUseCase(IBusinessPartnerRepository businessPartnerRepository, IUnitOfWork unitOfWork)
        {
            _businessPartnerRepository = businessPartnerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(Guid id, BusinessPartnerRole role)
        {
            if (!Enum.IsDefined(role))
                return Result.Failure(["El rol es inválido."], ErrorType.BadRequest);

            var businessPartner = await _businessPartnerRepository.GetByIdAsync(id);
            if (businessPartner is null)
                return Result.Failure(["El cliente o proveedor no existe."], ErrorType.NotFound);

            // La misma regla del dominio, revisada antes para responder con el mensaje: ya tiene ese rol o su documento
            // no sirve para él.
            var error = role == BusinessPartnerRole.Client ? businessPartner.AddClientRoleError() : businessPartner.AddSupplierRoleError();
            if (error is not null)
                return Result.Failure([error], ErrorType.BadRequest);

            if (role == BusinessPartnerRole.Client)
                businessPartner.AddClientRole();
            else
                businessPartner.AddSupplierRole();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
