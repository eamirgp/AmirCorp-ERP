using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.UnitsOfMeasure.UpdateUnitOfMeasure
{
    internal sealed class UpdateUnitOfMeasureUseCase : IUpdateUnitOfMeasureUseCase
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUnitOfMeasureUseCase(IUnitOfMeasureRepository unitOfMeasureRepository, IUnitOfWork unitOfWork)
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(UpdateUnitOfMeasureDto request)
        {
            var unit = await _unitOfMeasureRepository.GetByIdAsync(request.Id);
            if (unit is null)
                return Result.Failure(["La unidad de medida no existe."], ErrorType.NotFound);

            if (_unitOfMeasureRepository.VersionOf(unit) != request.RowVersion)
                return Result.Failure(
                    [ConcurrencyException.EditedWhileOpenMessage("esta unidad")],
                    ErrorType.Conflict
                    );

            // Todo el catálogo (son pocas): el nombre no puede confundirse con el de otra unidad.
            var units = await _unitOfMeasureRepository.ListAllAsync();

            if (UnitOfMeasure.NameError(request.Name, unit.Id, units) is { } nameError)
                return Result.Failure([nameError], ErrorType.Conflict);

            unit.UpdateName(request.Name, units);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
