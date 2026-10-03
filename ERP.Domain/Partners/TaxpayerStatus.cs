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
    }
}
