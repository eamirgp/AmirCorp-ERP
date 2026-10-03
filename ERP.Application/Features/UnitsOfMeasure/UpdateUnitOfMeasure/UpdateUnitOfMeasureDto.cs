namespace ERP.Application.Features.UnitsOfMeasure.UpdateUnitOfMeasure
{
    /// <summary>Solo el nombre corto se puede cambiar: el código y el nombre oficial son de SUNAT.</summary>
    /// <param name="RowVersion">La versión que se vio en la lista.</param>
    public sealed record UpdateUnitOfMeasureDto(Guid Id, string Name, uint RowVersion);
}
