using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.UnitsOfMeasure.UpdateUnitOfMeasure
{
    /// <summary>Solo el nombre corto se puede cambiar: el código y el nombre oficial son de SUNAT.</summary>
    public sealed record UpdateUnitOfMeasureDto(Guid Id, string Name);

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

            unit.UpdateName(request.Name);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
