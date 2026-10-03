namespace ERP.Application.Common.Responses
{
    /// <summary>Un archivo para descargar, con el nombre con que se guarda ("productos-2026-10-03.xlsx").</summary>
    public sealed record FileDto(byte[] Content, string FileName);
}
