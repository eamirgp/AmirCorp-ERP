using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.SavedViews.CreateSavedView
{
    public interface ICreateSavedViewUseCase : IUseCase<CreateSavedViewDto, Result<CreatedResponseDto>> { }
}
