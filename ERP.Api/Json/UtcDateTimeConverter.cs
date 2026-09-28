using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERP.Api.Json
{
    // Serializa todos los DateTime en UTC con sufijo "Z" (ISO 8601), para que el
    // frontend pueda hacer new Date(value) sin trucos. Las fechas que vienen de
    // SQL llegan con Kind=Unspecified (sin "Z"); aquí las tratamos como UTC.
    //
    // System.Text.Json reutiliza este converter automáticamente para DateTime?,
    // así que no hace falta registrar uno aparte para los anulables.
    internal sealed class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var value = reader.GetDateTime();
            return value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options)
        {
            var utc = value.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => value,
            };

            writer.WriteStringValue(
                utc.ToString("yyyy-MM-ddTHH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
        }
    }
}