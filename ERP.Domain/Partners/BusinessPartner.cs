using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Partners.Enums;

namespace ERP.Domain.Partners
{
    /// <summary>
    /// Cliente o proveedor (o los dos): un solo registro por contribuyente, como el Business Partner de SAP.
    /// Las reglas se exponen como funciones <c>…Error</c> que devuelven el mensaje o null: el dominio lanza el error con
    /// ellas y la API las usa para avisar todos los errores juntos.
    /// </summary>
    public sealed class BusinessPartner : AuditableEntity
    {
        // Holgado a propósito: las razones sociales de SUNAT (consorcios, asociaciones) pueden pasar de 100 caracteres.
        public const int NameMaxLength = 200;
        public const int BlockReasonMaxLength = 200;

        public IdentityDocumentType IdentityDocumentType { get; private set; }
        public string DocumentNumber { get; private set; }
        /// <summary>Código ISO del país (catálogo N.° 04 de SUNAT): PE con DNI o RUC; otro con documento extranjero.</summary>
        public string CountryCode { get; private set; }
        public string Name { get; private set; }
        public bool IsClient { get; private set; }
        public bool IsSupplier { get; private set; }

        // Cada rol se bloquea por separado, como el bloqueo de compras y el de ventas del Business Partner de SAP:
        // dejar de comprarle a alguien no impide seguir vendiéndole. No hay un "desactivar" general.
        public bool IsPurchasingBlocked { get; private set; }
        public string? PurchasingBlockReason { get; private set; }
        public bool IsSalesBlocked { get; private set; }
        public string? SalesBlockReason { get; private set; }

        private BusinessPartner(Guid id, IdentityDocumentType identityDocumentType, string documentNumber, string countryCode, string name, bool isClient, bool isSupplier) : base(id)
        {
            IdentityDocumentType = identityDocumentType;
            DocumentNumber = documentNumber;
            CountryCode = countryCode;
            Name = name;
            IsClient = isClient;
            IsSupplier = isSupplier;
        }

        public static BusinessPartner Create(IdentityDocumentType identityDocumentType, string documentNumber, string countryCode, string name, bool isClient, bool isSupplier)
        {
            var (normalizedDocument, normalizedCountry, normalizedName) = ValidateData(identityDocumentType, documentNumber, countryCode, name);
            Throw(RolesError(identityDocumentType, isClient, isSupplier));

            return new(Guid.CreateVersion7(), identityDocumentType, normalizedDocument, normalizedCountry, normalizedName, isClient, isSupplier);
        }

        /// <summary>
        /// Corrige los datos. Los roles no cambian aquí (se agregan con <see cref="AddClientRole"/> y
        /// <see cref="AddSupplierRole"/>), pero el documento nuevo debe servir para los roles que ya tiene.
        /// </summary>
        /// <param name="hasPurchases">Si ya tiene compras registradas (también las anuladas): entonces el documento no cambia.</param>
        public void Update(IdentityDocumentType identityDocumentType, string documentNumber, string countryCode, string name, bool hasPurchases)
        {
            var (normalizedDocument, normalizedCountry, normalizedName) = ValidateData(identityDocumentType, documentNumber, countryCode, name);
            Throw(RolesError(identityDocumentType, IsClient, IsSupplier));
            Throw(DocumentChangeError(identityDocumentType, normalizedDocument, hasPurchases));

            IdentityDocumentType = identityDocumentType;
            DocumentNumber = normalizedDocument;
            CountryCode = normalizedCountry;
            Name = normalizedName;
        }

        /// <summary>Un proveedor pasa a ser también cliente (si su documento lo permite).</summary>
        public void AddClientRole()
        {
            Throw(AddClientRoleError());
            IsClient = true;
        }

        /// <summary>Un cliente pasa a ser también proveedor (si su documento lo permite).</summary>
        public void AddSupplierRole()
        {
            Throw(AddSupplierRoleError());
            IsSupplier = true;
        }

        /// <summary>Deja de comprarle: ya no se puede elegir en compras nuevas. Las compras hechas no cambian.</summary>
        public void BlockPurchasing(string? reason)
        {
            Throw(BlockPurchasingError(reason));
            PurchasingBlockReason = NormalizeReason(reason);
            IsPurchasingBlocked = true;
        }

        /// <summary>Si no estaba bloqueado, no cambia nada (como archivar algo ya archivado en Odoo).</summary>
        public void UnblockPurchasing()
        {
            IsPurchasingBlocked = false;
            PurchasingBlockReason = null;
        }

        /// <summary>Deja de venderle: ya no se podrá elegir en ventas nuevas. Las ventas hechas no cambian.</summary>
        public void BlockSales(string? reason)
        {
            Throw(BlockSalesError(reason));
            SalesBlockReason = NormalizeReason(reason);
            IsSalesBlocked = true;
        }

        /// <summary>Si no estaba bloqueado, no cambia nada.</summary>
        public void UnblockSales()
        {
            IsSalesBlocked = false;
            SalesBlockReason = null;
        }

        /// <summary>"Las compras a ACME S.A.C. están bloqueadas (motivo: …).", o null si no lo están.</summary>
        public string? PurchasingBlockedError() =>
            IsPurchasingBlocked
                ? $"Las compras a {Name} están bloqueadas" + (PurchasingBlockReason is { } reason ? $" (motivo: {reason})." : ".")
                : null;

        /// <summary>El nombre tal como se guarda: sin espacios al inicio ni al final, ni dobles en medio.</summary>
        public static string NormalizeName(string name) =>
            TextNormalizer.CollapseSpaces(name);

        /// <summary>País que corresponde al documento: DNI y RUC son siempre de Perú; el extranjero, el que se indique.</summary>
        public static string? CountryFor(IdentityDocumentType identityDocumentType, string? countryCode) =>
            identityDocumentType.RequiresPeruvianCountry ? Countries.Peru : countryCode;

        /// <summary>Si se le puede agregar el rol de cliente: no lo es todavía y su documento lo permite.</summary>
        public static bool CanAddClientRole(IdentityDocumentType identityDocumentType, bool isClient) =>
            !isClient && identityDocumentType.CanBeClient;

        /// <summary>Si se le puede agregar el rol de proveedor: no lo es todavía y su documento lo permite.</summary>
        public static bool CanAddSupplierRole(IdentityDocumentType identityDocumentType, bool isSupplier) =>
            !isSupplier && identityDocumentType.CanIssueTaxDocuments;

        /// <summary>Qué tiene de malo el documento, o null si es válido. El RUC se valida como SUNAT.</summary>
        public static string? DocumentError(IdentityDocumentType? identityDocumentType, string? documentNumber) =>
            identityDocumentType switch
            {
                null => "El tipo de documento es requerido.",
                { } type when !Enum.IsDefined(type) => "El tipo de documento es inválido.",
                { } type => type.DocumentNumberError(IdentityDocumentTypeExtensions.NormalizeDocumentNumber(documentNumber ?? ""))
            };

        /// <summary>
        /// Qué tiene de malo el país, o null si está bien. Con DNI o RUC es Perú (<see cref="CountryFor"/>); con un
        /// documento extranjero se elige y no puede ser Perú.
        /// </summary>
        public static string? CountryError(IdentityDocumentType identityDocumentType, string? countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return "Elige el país del proveedor.";

            if (!Countries.Exists(countryCode))
                return "El país es inválido.";

            var isPeru = Countries.IsPeru(countryCode);

            if (identityDocumentType.RequiresPeruvianCountry && !isPeru)
                return "Para DNI o RUC el país debe ser Perú.";

            if (!identityDocumentType.RequiresPeruvianCountry && isPeru)
                return "Un documento extranjero no puede ser de Perú. Elige el país del proveedor.";

            return null;
        }

        /// <summary>Qué tiene de malo el nombre o razón social, o null si está bien.</summary>
        public static string? NameError(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "El nombre es requerido.";

            if (NormalizeName(name).Length > NameMaxLength)
                return $"El nombre no puede exceder los {NameMaxLength} caracteres.";

            return null;
        }

        /// <summary>
        /// Qué tienen de malo los roles para ese documento, o null si están bien: al menos uno, un proveedor emite
        /// facturas (RUC o documento extranjero) y por ahora solo se vende en Perú (RUC o DNI).
        /// </summary>
        /// <param name="identityDocumentType">Null o inválido: solo se revisa que haya un rol (el documento ya tiene su error).</param>
        public static string? RolesError(IdentityDocumentType? identityDocumentType, bool isClient, bool isSupplier)
        {
            if (!isClient && !isSupplier)
                return "Marca si es cliente, proveedor o ambos.";

            if (identityDocumentType is not { } type || !Enum.IsDefined(type))
                return null;

            if (isSupplier && !type.CanIssueTaxDocuments)
                return "Un proveedor debe tener RUC o documento extranjero: con DNI no puede emitir facturas.";

            if (isClient && !type.CanBeClient)
                return "Por ahora solo se vende en Perú: un cliente debe tener RUC o DNI.";

            return null;
        }

        /// <summary>Qué tiene de malo el motivo de un bloqueo, o null si está bien. Es opcional.</summary>
        public static string? BlockReasonError(string? reason) =>
            !string.IsNullOrWhiteSpace(reason) && TextNormalizer.CollapseSpaces(reason).Length > BlockReasonMaxLength
                ? $"El motivo no puede exceder los {BlockReasonMaxLength} caracteres."
                : null;

        /// <summary>
        /// Qué impide cambiar el documento por ese, o null si se puede (o si no cambia). El documento identifica al
        /// contribuyente: con compras registradas, cambiarlo mezclaría dos empresas. Cada compra guarda su copia.
        /// </summary>
        public string? DocumentChangeError(IdentityDocumentType identityDocumentType, string documentNumber, bool hasPurchases)
        {
            var changes = identityDocumentType != IdentityDocumentType
                || IdentityDocumentTypeExtensions.NormalizeDocumentNumber(documentNumber) != DocumentNumber;

            return hasPurchases && changes
                ? $"{Name} ya tiene compras registradas con {IdentityDocumentType.Description} {DocumentNumber}, así que el documento no se puede cambiar. " +
                  "Si es otra empresa, regístrala como un cliente o proveedor nuevo."
                : null;
        }

        /// <summary>Qué impide agregarle el rol de cliente, o null si se puede.</summary>
        public string? AddClientRoleError() =>
            IsClient ? $"{Name} ya es cliente." : RolesError(IdentityDocumentType, isClient: true, IsSupplier);

        /// <summary>Qué impide agregarle el rol de proveedor, o null si se puede.</summary>
        public string? AddSupplierRoleError() =>
            IsSupplier ? $"{Name} ya es proveedor." : RolesError(IdentityDocumentType, IsClient, isSupplier: true);

        /// <summary>Qué impide bloquear sus compras con ese motivo, o null si se puede. Bloquear de nuevo cambia el motivo.</summary>
        public string? BlockPurchasingError(string? reason) =>
            IsSupplier ? BlockReasonError(reason) : $"{Name} no es proveedor.";

        /// <summary>Qué impide bloquear sus ventas con ese motivo, o null si se puede. Bloquear de nuevo cambia el motivo.</summary>
        public string? BlockSalesError(string? reason) =>
            IsClient ? BlockReasonError(reason) : $"{Name} no es cliente.";

        private static (string Document, string Country, string Name) ValidateData(IdentityDocumentType identityDocumentType, string documentNumber, string countryCode, string name)
        {
            Throw(DocumentError(identityDocumentType, documentNumber));
            Throw(CountryError(identityDocumentType, countryCode));
            Throw(NameError(name));

            return (
                IdentityDocumentTypeExtensions.NormalizeDocumentNumber(documentNumber),
                Countries.NormalizeCode(countryCode),
                NormalizeName(name)
                );
        }

        // El motivo es opcional: vacío cuenta como sin motivo.
        private static string? NormalizeReason(string? reason) =>
            string.IsNullOrWhiteSpace(reason) ? null : TextNormalizer.CollapseSpaces(reason);

        private static void Throw(string? error)
        {
            if (error is not null)
                throw new DomainException(error);
        }
    }
}
