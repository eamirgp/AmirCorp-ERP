using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Companies;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;
using ERP.Domain.Products;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Domain.Purchases
{
    public sealed class Purchase : AuditableEntity
    {
        public const int SerieMaxLength = 4;
        public const int NumberMaxLength = 8;
        public const int CancellationReasonMaxLength = 200;

        private static readonly TimeZoneInfo PeruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");

        public Guid CompanyId { get; }
        /// <summary>Copia del RUC y la razón social de la empresa al registrar la compra (el comprador del comprobante).</summary>
        public string CompanyRuc { get; }
        public string CompanyName { get; }
        public TaxDocumentType TaxDocumentType { get; }
        public string Serie { get; }
        public string Number { get; }
        public DateOnly IssueDate { get; }
        public Currency Currency { get; }
        public decimal? ExchangeRate { get; }
        public InvoicePriceType InvoicePriceType { get; }
        public Guid SupplierId { get; }
        public IdentityDocumentType SupplierIdentityDocumentType { get; }
        public string SupplierDocumentNumber { get; }
        public string SupplierName { get; }
        public decimal TotalBaseAmount { get; private set; }
        public decimal TotalIgvAmount { get; private set; }
        public decimal Total { get; private set; }
        public bool IsCancelled { get; private set; }
        public string? CancellationReason { get; private set; }

        private readonly List<PurchaseLine> _lines = [];
        public IReadOnlyCollection<PurchaseLine> Lines => _lines.AsReadOnly();

        private Purchase(
            Guid id,
            Guid companyId,
            string companyRuc,
            string companyName,
            TaxDocumentType taxDocumentType,
            string serie,
            string number,
            DateOnly issueDate,
            Currency currency,
            decimal? exchangeRate,
            InvoicePriceType invoicePriceType,
            Guid supplierId,
            IdentityDocumentType supplierIdentityDocumentType,
            string supplierDocumentNumber,
            string supplierName
            ) : base(id)
        {
            CompanyId = companyId;
            CompanyRuc = companyRuc;
            CompanyName = companyName;
            TaxDocumentType = taxDocumentType;
            Serie = serie;
            Number = number;
            IssueDate = issueDate;
            Currency = currency;
            ExchangeRate = exchangeRate;
            InvoicePriceType = invoicePriceType;
            SupplierId = supplierId;
            SupplierIdentityDocumentType = supplierIdentityDocumentType;
            SupplierDocumentNumber = supplierDocumentNumber;
            SupplierName = supplierName;
        }

        /// <summary>
        /// Registra la compra que la empresa le hace al proveedor. Guarda una copia del RUC y la razón social que la
        /// empresa y el proveedor tienen hoy: si después cambian, la compra sigue mostrando lo que decía su comprobante.
        /// </summary>
        public static Purchase Create(
            Company company,
            BusinessPartner supplier,
            TaxDocumentType taxDocumentType,
            string serie,
            string number,
            DateOnly issueDate,
            Currency currency,
            decimal? exchangeRate,
            InvoicePriceType invoicePriceType
            )
        {
            if (PartiesError(company, supplier) is { } partiesError)
                throw new DomainException(partiesError);
            ValidateTaxDocumentType(taxDocumentType);
            var normalizedSerie = ValidateSerie(serie);
            if (SerieError(taxDocumentType, normalizedSerie) is { } serieError)
                throw new DomainException(serieError);
            var normalizedNumber = ValidateNumber(number);
            ValidateIssueDate(issueDate);
            ValidateCurrency(currency);
            ValidateExchangeRate(currency, exchangeRate);
            ValidateInvoicePriceType(invoicePriceType);

            return new(
                Guid.CreateVersion7(),
                company.Id,
                company.Ruc,
                company.Name,
                taxDocumentType,
                normalizedSerie,
                normalizedNumber,
                issueDate,
                currency,
                exchangeRate,
                invoicePriceType,
                supplier.Id,
                supplier.IdentityDocumentType,
                supplier.DocumentNumber,
                supplier.Name
                );
        }

        /// <summary>
        /// Qué impide que la empresa le compre a ese proveedor, o null si puede. La usa <see cref="Create"/> y el
        /// registro de la compra para responder con un mensaje claro antes de crearla.
        /// </summary>
        public static string? PartiesError(Company company, BusinessPartner supplier)
        {
            if (!company.IsActive)
                return "La empresa está desactivada.";

            if (!supplier.IsSupplier)
                return "El cliente elegido no está registrado como proveedor.";

            // Una empresa no se compra a sí misma: casi siempre es el RUC propio escrito por error en vez del proveedor.
            if (supplier.IdentityDocumentType.IsDomesticTaxpayer && supplier.DocumentNumber == company.Ruc)
                return $"El proveedor tiene el mismo RUC que la empresa que compra ({company.Ruc}). Revisa cuál de los dos está mal elegido.";

            if (supplier.PurchasingBlockedError() is { } blocked)
                return blocked;

            if (!supplier.IdentityDocumentType.IsDomesticTaxpayer)
                return "El proveedor de una compra nacional debe tener RUC.";

            return null;
        }

        /// <summary>
        /// Agrega el producto con lo que dice la factura. La línea guarda una copia del código interno y el nombre del
        /// producto, y el código que este proveedor usa para él (null si no tiene uno enlazado).
        /// </summary>
        public PurchaseLine AddLine(
            Product product,
            IgvAffectation invoiceIgvAffectation,
            UnitOfMeasure invoiceUnitOfMeasure,
            decimal invoiceQuantity,
            decimal invoiceAmount,
            // Null con una unidad de cantidad fija (Unidad, Docena): la pone el catálogo.
            decimal? conversionFactor
            )
        {
            if (ProductError(product) is { } productError)
                throw new DomainException(productError);

            if (_lines.Any(l => l.ProductId == product.Id))
                throw new DomainException($"El producto {product.Code} está más de una vez en la compra.");

            var purchaseLine = PurchaseLine.Create(
                Id,
                _lines.Count + 1,
                product.Id,
                product.Code,
                product.Name,
                product.SupplierCodeOf(SupplierId)?.Code,
                InvoicePriceType,
                invoiceIgvAffectation,
                invoiceUnitOfMeasure,
                invoiceQuantity,
                invoiceAmount,
                conversionFactor
                );

            // Un código del proveedor es un solo producto, y un producto va en una sola línea.
            if (purchaseLine.SupplierProductCode is { } code && _lines.Any(l => l.SupplierProductCode == code))
                throw new DomainException($"El código {code} del proveedor está en más de una línea de la compra.");

            _lines.Add(purchaseLine);
            RecalculateTotals();
            return purchaseLine;
        }

        /// <summary>
        /// Por qué no se puede registrar un comprobante que ya está registrado (no anulado). Un comprobante del proveedor
        /// va a un solo RUC: no se registra dos veces, ni en dos empresas propias.
        /// </summary>
        /// <param name="companyId">La empresa que lo quiere registrar.</param>
        /// <param name="registeredCompanyId">La empresa en la que ya está.</param>
        public static string DuplicateDocumentError(Guid companyId, Guid registeredCompanyId, string registeredCompanyName) =>
            registeredCompanyId == companyId
                ? "El comprobante ya se encuentra registrado para este proveedor."
                // El nombre no cierra la oración: las razones sociales suelen terminar en punto ("E.I.R.L.").
                : $"La empresa {registeredCompanyName} ya tiene registrado este comprobante, y un comprobante del proveedor va a una sola empresa. Revisa cuál hizo la compra.";

        /// <summary>Qué impide comprar el producto, o null si se puede.</summary>
        public static string? ProductError(Product product) =>
            product.IsActive ? null : $"El producto '{product.Name}' está desactivado.";

        public void EnsureHasLines()
        {
            if (_lines.Count == 0)
                throw new DomainException("La compra debe tener al menos una línea.");
        }

        /// <summary>
        /// Totales de la compra a partir de los montos de sus líneas. La usan el registro y la vista previa.
        /// </summary>
        public static PurchaseTotals CalculateTotals(IEnumerable<(decimal BaseAmount, decimal IgvAmount)> lines)
        {
            var list = lines.ToList();
            var totalBaseAmount = list.Sum(l => l.BaseAmount);
            var totalIgvAmount = list.Sum(l => l.IgvAmount);
            return new PurchaseTotals(totalBaseAmount, totalIgvAmount, totalBaseAmount + totalIgvAmount);
        }

        private void RecalculateTotals()
        {
            var totals = CalculateTotals(_lines.Select(l => (l.BaseAmount, l.IgvAmount)));
            TotalBaseAmount = totals.TotalBaseAmount;
            TotalIgvAmount = totals.TotalIgvAmount;
            Total = totals.Total;
        }

        public void Cancel(string cancellationReason)
        {
            if (IsCancelled)
                throw new DomainException("La compra ya se encuentra anulada.");

            CancellationReason = ValidateCancellationReason(cancellationReason);
            IsCancelled = true;
        }

        // Sin espacios alrededor: un "F001 " pegado de otro lado es la serie F001.
        public static string NormalizeSerie(string serie) =>
            serie.Trim().ToUpperInvariant();

        public static string NormalizeNumber(string number) =>
            number.Trim().PadLeft(NumberMaxLength, '0');

        private static void ValidateTaxDocumentType(TaxDocumentType taxDocumentType)
        {
            if (!Enum.IsDefined(taxDocumentType))
                throw new DomainException("El tipo de documento es inválido.");
        }

        private static string ValidateSerie(string serie)
        {
            if (string.IsNullOrWhiteSpace(serie))
                throw new DomainException("La serie es requerida.");

            serie = serie.Trim();

            if (serie.Length != SerieMaxLength)
                throw new DomainException($"La serie debe tener exactamente {SerieMaxLength} caracteres.");

            if (!serie.All(char.IsLetterOrDigit))
                throw new DomainException("La serie debe contener solo letras y números.");

            return NormalizeSerie(serie);
        }

        /// <summary>
        /// Qué tiene de malo la serie para ese comprobante, o null si corresponde. Los electrónicos empiezan con la
        /// letra de su tipo (factura F001 o E001; boleta B001 o EB01) y los físicos son numéricos (0001). Una factura
        /// con serie B, o una boleta con serie F, es un error de tipeo o un comprobante mal elegido.
        /// </summary>
        public static string? SerieError(TaxDocumentType taxDocumentType, string serie)
        {
            var normalized = NormalizeSerie(serie);
            if (normalized.All(char.IsDigit))
                return null;

            return taxDocumentType switch
            {
                TaxDocumentType.Factura when normalized[0] is not ('F' or 'E') || normalized.StartsWith("EB") =>
                    $"La serie {normalized} no es de una factura: debe empezar con F (o E), o ser numérica si es física.",
                TaxDocumentType.Boleta when normalized[0] != 'B' && !normalized.StartsWith("EB") =>
                    $"La serie {normalized} no es de una boleta: debe empezar con B (o EB), o ser numérica si es física.",
                _ => null
            };
        }

        private static string ValidateNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new DomainException("El número es requerido.");

            var numberNormalized = NormalizeNumber(number);

            if (numberNormalized.Length > NumberMaxLength)
                throw new DomainException($"El número no puede exceder los {NumberMaxLength} dígitos.");

            if (!numberNormalized.All(char.IsDigit))
                throw new DomainException("El número debe contener solo dígitos.");

            if (numberNormalized.All(c => c == '0'))
                throw new DomainException("El número debe ser mayor a cero.");

            return numberNormalized;
        }

        private static void ValidateIssueDate(DateOnly issueDate)
        {
            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PeruTimeZone));

            if (issueDate > today)
                throw new DomainException("La fecha de emisión no puede ser mayor a la fecha actual.");
        }

        private static void ValidateCurrency(Currency currency)
        {
            if (!Enum.IsDefined(currency))
                throw new DomainException("La moneda es inválida.");
        }

        private static void ValidateExchangeRate(Currency currency, decimal? exchangeRate)
        {
            if (currency is Currency.PEN && exchangeRate is not null)
                throw new DomainException("El tipo de cambio no aplica para soles.");

            if (currency is not Currency.PEN && exchangeRate is null)
                throw new DomainException("El tipo de cambio es requerido para moneda extranjera.");

            if (exchangeRate is not null && exchangeRate.Value <= 0)
                throw new DomainException("El tipo de cambio debe ser mayor a cero.");
        }

        private static void ValidateInvoicePriceType(InvoicePriceType invoicePriceType)
        {
            if (!Enum.IsDefined(invoicePriceType))
                throw new DomainException("El tipo de precio es inválido.");
        }

        private static string ValidateCancellationReason(string cancellationReason)
        {
            if (string.IsNullOrWhiteSpace(cancellationReason))
                throw new DomainException("El motivo de anulación es requerido.");

            var normalized = TextNormalizer.CollapseSpaces(cancellationReason);
            if (normalized.Length > CancellationReasonMaxLength)
                throw new DomainException($"El motivo de anulación no puede exceder los {CancellationReasonMaxLength} caracteres.");

            return normalized;
        }
    }
}
