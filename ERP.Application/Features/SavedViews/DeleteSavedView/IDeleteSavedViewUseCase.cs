using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.SavedViews.DeleteSavedView
{
    public interface IDeleteSavedViewUseCase : IUseCase<DeleteSavedViewDto, Result> { }
}
