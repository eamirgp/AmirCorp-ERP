using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.UnitsOfMeasure.ToggleUnitOfMeasure
{
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
}
