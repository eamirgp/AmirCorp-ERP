namespace ERP.Domain.Partners
{
    /// <summary>Estado y condición de un contribuyente según SUNAT.</summary>
    public static class TaxpayerStatus
    {
        public const string Active = "ACTIVO";
        public const string Located = "HABIDO";

        /// <summary>
        /// Si SUNAT lo tiene en regla: ACTIVO y HABIDO. Los comprobantes de un contribuyente de baja o no habido pueden
        /// no servir para el crédito fiscal del IGV.
        /// </summary>
        public static bool IsActiveAndLocated(string status, string condition) =>
            status.Trim().Equals(Active, StringComparison.OrdinalIgnoreCase)
            && condition.Trim().Equals(Located, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// "Según SUNAT está DE BAJA y NO HABIDO", sin dejar huecos si SUNAT no envió alguno de los dos datos (antes salía
        /// "está  y ."). Cada pantalla agrega después qué revisar.
        /// </summary>
        public static string Describe(string status, string condition)
        {
            var parts = new[] { status.Trim(), condition.Trim() }.Where(p => p.Length > 0).ToArray();
            return parts.Length == 0
                ? "SUNAT no informa si está activo y habido"
                : $"Según SUNAT está {string.Join(" y ", parts)}";
        }
    }
}
