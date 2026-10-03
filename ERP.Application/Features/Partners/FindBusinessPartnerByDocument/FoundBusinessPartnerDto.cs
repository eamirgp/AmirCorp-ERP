namespace ERP.Application.Features.Partners.FindBusinessPartnerByDocument
{
    /// <summary>
    /// Quién tiene ya ese documento y qué se puede hacer: si está en la otra lista, el formulario ofrece agregarlo
    /// también a esta en vez de crear un duplicado. Los textos vienen listos (la pantalla no arma frases).
    /// </summary>
    public sealed record FoundBusinessPartnerDto(
        Guid Id,
        string Name,
        bool IsClient,
        bool IsSupplier,
        // Si se le puede agregar ese rol: no lo tiene todavía y su documento lo permite. Un bloqueo en el otro rol
        // no impide agregarlo: cada rol se bloquea por separado.
        bool CanAddClientRole,
        bool CanAddSupplierRole,
        // Por qué no se le puede agregar ese rol ("X ya es cliente.", "Por ahora solo se vende en Perú…"), o null si se puede.
        string? AddClientRoleError,
        string? AddSupplierRoleError
        )
    {
        public string RoleDescription => BusinessPartnerRules.RoleDescription(IsClient, IsSupplier);

        /// <summary>"ACME S.A.C. ya está registrado como proveedor."</summary>
        public string Summary => $"{Name} ya está registrado como {RoleDescription.ToLowerInvariant()}.";
    }
}
