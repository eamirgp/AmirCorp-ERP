namespace ERP.Application.Common.Results
{
    /// <summary>
    /// Un error para el usuario y, si es de un campo del formulario, cuál (decisión 37): así la pantalla lo pone debajo de
    /// ese campo, como Apple, en vez de en una lista arriba. Sin campo, el error es del formulario entero (por ejemplo,
    /// "otra persona cambió este producto").
    /// </summary>
    /// <param name="Field">
    /// El campo como se llama en el pedido, en camelCase: <c>code</c>, <c>supplierCodes[1].code</c> (filas desde 0).
    /// Se arma con <see cref="FieldName"/>.
    /// </param>
    public sealed record ErrorDetail(string Message, string? Field = null);

    /// <summary>
    /// El nombre de un campo como lo envía y lo recibe la pantalla (el JSON usa camelCase): <c>FieldName.Of(nameof(Code))</c>
    /// es <c>code</c>. Los casos de uso nombran los campos por las propiedades de su DTO, que son las del pedido.
    /// </summary>
    public static class FieldName
    {
        public static string Of(string property) =>
            char.ToLowerInvariant(property[0]) + property[1..];

        /// <summary>El campo de una fila de una lista: <c>supplierCodes[1].code</c>, con la fila contada desde 0.</summary>
        public static string Item(string list, int index, string property) =>
            $"{Of(list)}[{index}].{Of(property)}";
    }
}
