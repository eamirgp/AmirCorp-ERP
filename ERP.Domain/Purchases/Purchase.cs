using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Partners.Enums;

namespace ERP.Domain.Purchases
{
    public sealed class Purchase : AuditableEntity
    {
        public const int SerieMaxLength = 4;
        public const int NumberMaxLength = 8;
        public const int CancellationReasonMaxLength = 200;

        private static readonly TimeZoneInfo PeruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");

        public Guid CompanyId { get; }
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

        public static Purchase Create(
            Guid companyId,
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
            )
        {
            ValidateCompany(companyId);
            ValidateTaxDocumentType(taxDocumentType);
            var normalizedSerie = ValidateSerie(serie);
            var normalizedNumber = ValidateNumber(number);
            ValidateIssueDate(issueDate);
            ValidateCurrency(currency);
            ValidateExchangeRate(currency, exchangeRate);
            ValidateInvoicePriceType(invoicePriceType);
            ValidateSupplier(supplierId, supplierIdentityDocumentType);

            return new(
                Guid.CreateVersion7(),
                companyId,
                taxDocumentType,
                normalizedSerie,
                normalizedNumber,
                issueDate,
                currency,
                exchangeRate,
                invoicePriceType,
                supplierId,
                supplierIdentityDocumentType,
                supplierDocumentNumber,
                supplierName
                );
        }

        public PurchaseLine AddLine(
            Guid productId,
            string productCode,
            string productName,
            IgvAffectation invoiceIgvAffectation,
            UnitOfMeasure invoiceUnitOfMeasure,
            decimal invoiceQuantity,
            decimal invoiceAmount,
            decimal conversionFactor
            )
        {
            if (_lines.Any(l => l.ProductId == productId))
                throw new DomainException($"El producto con ID '{productId}' está duplicado en la compra.");

            var purchaseLine = PurchaseLine.Create(
                Id,
                _lines.Count + 1,
                productId,
                productCode,
                productName,
                InvoicePriceType,
                invoiceIgvAffectation,
                invoiceUnitOfMeasure,
                invoiceQuantity,
                invoiceAmount,
                conversionFactor
                );

            _lines.Add(purchaseLine);
            RecalculateTotals();
            return purchaseLine;
        }

        public void EnsureHasLines()
        {
            if (_lines.Count == 0)
                throw new DomainException("La compra debe tener al menos una línea.");
        }

        private void RecalculateTotals()
        {
            TotalBaseAmount = _lines.Sum(l => l.BaseAmount);
            TotalIgvAmount = _lines.Sum(l => l.IgvAmount);
            Total = TotalBaseAmount + TotalIgvAmount;
        }

        public void Cancel(string cancellationReason)
        {
            if (IsCancelled)
                throw new DomainException("La compra ya se encuentra anulada.");

            CancellationReason = ValidateCancellationReason(cancellationReason);
            IsCancelled = true;
        }

        public static string NormalizeSerie(string serie) =>
            serie.ToUpperInvariant();

        public static string NormalizeNumber(string number) =>
            number.PadLeft(NumberMaxLength, '0');

        private static void ValidateCompany(Guid companyId)
        {
            if (companyId == Guid.Empty)
                throw new DomainException("La empresa es requerida.");
        }

        private static void ValidateTaxDocumentType(TaxDocumentType taxDocumentType)
        {
            if (!Enum.IsDefined(taxDocumentType))
                throw new DomainException("El tipo de documento es inválido.");
        }

        private static string ValidateSerie(string serie)
        {
            if (string.IsNullOrWhiteSpace(serie))
                throw new DomainException("La serie es requerida.");

            if (serie.Length != SerieMaxLength)
                throw new DomainException($"La serie debe tener exactamente {SerieMaxLength} caracteres.");

            if (!serie.All(char.IsLetterOrDigit))
                throw new DomainException("La serie debe contener solo letras y números.");

            return NormalizeSerie(serie);
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

        private static void ValidateSupplier(Guid supplierId, IdentityDocumentType supplierIdentityDocumentType)
        {
            if (supplierId == Guid.Empty)
                throw new DomainException("El proveedor es requerido.");

            if (!supplierIdentityDocumentType.IsDomesticTaxpayer)
                throw new DomainException("El proveedor de una compra nacional debe tener RUC.");
        }

        private static string ValidateCancellationReason(string cancellationReason)
        {
            if (string.IsNullOrWhiteSpace(cancellationReason))
                throw new DomainException("El motivo de anulación es requerido.");

            if (cancellationReason.Length > CancellationReasonMaxLength)
                throw new DomainException($"El motivo de anulación no puede exceder los {CancellationReasonMaxLength} caracteres.");

            return cancellationReason;
        }
    }
}
