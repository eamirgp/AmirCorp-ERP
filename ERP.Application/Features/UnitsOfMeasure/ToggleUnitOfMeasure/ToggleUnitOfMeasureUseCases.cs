using ERP.Application.Common.Formatting;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.UnitsOfMeasure.ToggleUnitOfMeasure
{
    public interface IActivateUnitOfMeasureUseCase
    {
        Task<Result> ExecuteAsync(Guid id);
    }

    public interface IDeactivateUnitOfMeasureUseCase
    {
        Task<Result> ExecuteAsync(Guid id);
    }

    /// <summary>Activar una unidad la hace aparecer en productos, compras y la planilla.</summary>
    internal sealed class ActivateUnitOfMeasureUseCase : IActivateUnitOfMeasureUseCase
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateUnitOfMeasureUseCase(IUnitOfMeasureRepository unitOfMeasureRepository, IUnitOfWork unitOfWork)
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(Guid id)
        {
            var unit = await _unitOfMeasureRepository.GetByIdAsync(id);
            if (unit is null)
                return Result.Failure(["La unidad de medida no existe."], ErrorType.NotFound);

            unit.Activate();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }

    /// <summary>
    /// Una unidad que usa algún producto no se puede desactivar: el producto quedaría con una unidad que ya no
    /// aparece en las listas. Primero hay que cambiarles la unidad a esos productos.
    /// </summary>
    internal sealed class DeactivateUnitOfMeasureUseCase : IDeactivateUnitOfMeasureUseCase
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateUnitOfMeasureUseCase(IUnitOfMeasureRepository unitOfMeasureRepository, IUnitOfWork unitOfWork)
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(Guid id)
        {
            var unit = await _unitOfMeasureRepository.GetByIdAsync(id);
            if (unit is null)
                return Result.Failure(["La unidad de medida no existe."], ErrorType.NotFound);

            var products = await _unitOfMeasureRepository.CountProductsUsingAsync(unit.Code);
            if (products > 0)
            {
                var count = products == 1 ? "1 producto" : $"{NumberText.Integer(products)} productos";
                return Result.Failure(
                    [$"No se puede desactivar {unit.Name}: la usan {count}. Cámbiales la unidad de medida y vuelve a intentarlo."],
                    ErrorType.Conflict
                    );
            }

            unit.Deactivate();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
