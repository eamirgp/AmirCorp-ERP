using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.UnitsOfMeasure.UpdateUnitOfMeasure
{
    /// <summary>Solo el nombre corto se puede cambiar: el código y el nombre oficial son de SUNAT.</summary>
    /// <param name="RowVersion">La versión que se vio en la lista.</param>
    public sealed record UpdateUnitOfMeasureDto(Guid Id, string Name, uint RowVersion);

    public interface IUpdateUnitOfMeasureUseCase
    {
        Task<Result> ExecuteAsync(UpdateUnitOfMeasureDto request);
    }

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
                    ["Otra persona modificó esta unidad mientras la editabas. Cierra el formulario y vuelve a abrirlo para ver los datos actuales."],
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
