namespace ERP.Domain.Common
{
    /// <summary>
    /// Fechas en hora de Perú (UTC-5, sin horario de verano). Un día de negocio (la fecha de un comprobante, el tipo de
    /// cambio de un día, un filtro "desde/hasta") es el de Lima, no el del servidor ni el UTC: a las 10 de la noche en Lima
    /// ya es el día siguiente en UTC.
    /// </summary>
    public static class PeruCalendar
    {
        public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");

        /// <summary>La fecha de hoy en Lima para ese instante (en UTC).</summary>
        public static DateOnly Today(DateTime utcNow) =>
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), TimeZone));

        /// <summary>El instante (en UTC) en que empieza ese día en Lima: las 00:00 de Lima son las 05:00 UTC.</summary>
        public static DateTime StartOfDayUtc(DateOnly day) =>
            TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), TimeZone);
    }
}
