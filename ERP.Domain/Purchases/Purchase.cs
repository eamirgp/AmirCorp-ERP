using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Companies;
using ERP.Domain.Inventory;
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
        // Los mismos límites del tipo de cambio guardado: más es un error de tipeo.
        public const decimal ExchangeRateMax = Catalogs.ExchangeRate.RateMax;
        public const int ExchangeRateDecimals = Catalogs.ExchangeRate.RateDecimals;
        private static readonly DateOnly MinIssueDate = new(2000, 1, 1);


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
            Throw(PartiesError(company, supplier));
            Throw(TaxDocumentTypeError(taxDocumentType));
            Throw(SerieError(taxDocumentType, serie));
            Throw(NumberError(number));
            Throw(IssueDateError(issueDate));
            Throw(CurrencyError(currency));
            Throw(ExchangeRateError(currency, exchangeRate));
            Throw(InvoicePriceTypeError(invoicePriceType));

            return new(
                Guid.CreateVersion7(),
                company.Id,
                company.Ruc,
                company.Name,
                taxDocumentType,
                NormalizeSerie(serie),
                NormalizeNumber(number),
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
            if (IsCancelled)
                throw new DomainException("No se pueden agregar productos a una compra anulada.");

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

        /// <summary>
        /// Anula la compra. Su mercadería sale del stock (quien llama quita sus ingresos), así que no se puede anular si
        /// ya tuvo salidas: se vendió o se movió algo de lo que trajo.
        /// </summary>
        /// <param name="stockEntries">Los ingresos de stock de las líneas de esta compra.</param>
        public void Cancel(string cancellationReason, IReadOnlyCollection<StockEntry> stockEntries)
        {
            Throw(CancelError(stockEntries));
            Throw(CancellationReasonError(cancellationReason));

            CancellationReason = TextNormalizer.CollapseSpaces(cancellationReason);
            IsCancelled = true;
        }

        /// <summary>Qué impide anular la compra, o null si se puede. El caso de uso la revisa antes para responder el mensaje.</summary>
        /// <param name="stockEntries">Los ingresos de stock de las líneas de esta compra.</param>
        public string? CancelError(IReadOnlyCollection<StockEntry> stockEntries)
        {
            if (IsCancelled)
                return "La compra ya se encuentra anulada.";

            if (stockEntries.Any(e => _lines.All(l => l.Id != e.PurchaseLineId)))
                throw new DomainException("Los ingresos de stock no son de esta compra.");

            if (stockEntries.Any(e => !e.IsIntact))
                return "No se puede anular la compra porque su mercadería ya tuvo movimientos de salida.";

            return null;
        }

        // Sin espacios alrededor: un "F001 " pegado de otro lado es la serie F001.
        public static string NormalizeSerie(string serie) =>
            serie.Trim().ToUpperInvariant();

        public static string NormalizeNumber(string number) =>
            number.Trim().PadLeft(NumberMaxLength, '0');

        // Las reglas de los datos del comprobante: devuelven el mensaje o null. El dominio lanza el error con ellas y la
        // API las llama antes para avisar todos los errores juntos (decisión 20).

        public static string? TaxDocumentTypeError(TaxDocumentType? taxDocumentType) =>
            taxDocumentType switch
            {
                null => "El tipo de documento es requerido.",
                { } value when !Enum.IsDefined(value) => "El tipo de documento es inválido.",
                _ => null
            };

        /// <summary>
        /// Qué tiene de malo la serie, o null si está bien: 4 letras o números, y que corresponda al comprobante. Los
        /// electrónicos empiezan con la letra de su tipo (factura F001 o E001; boleta B001 o EB01) y los físicos son
        /// numéricos (0001). Una factura con serie B, o una boleta con serie F, es un error de tipeo o un comprobante mal
        /// elegido. Sin tipo de comprobante (o con uno inválido) solo se revisa el formato.
        /// </summary>
        public static string? SerieError(TaxDocumentType? taxDocumentType, string? serie)
        {
            if (string.IsNullOrWhiteSpace(serie))
                return "La serie es requerida.";

            var normalized = NormalizeSerie(serie);
            if (normalized.Length != SerieMaxLength)
                return $"La serie debe tener exactamente {SerieMaxLength} caracteres.";

            // Solo letras sin tilde y números, como los acepta SUNAT (no "Ñ" ni dígitos de otros alfabetos).
            if (!normalized.All(char.IsAsciiLetterOrDigit))
                return "La serie debe contener solo letras y números.";

            if (normalized.All(char.IsAsciiDigit))
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

        public static string? NumberError(string? number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return "El número es requerido.";

            var trimmed = number.Trim();
            if (!trimmed.All(char.IsAsciiDigit))
                return "El número debe contener solo dígitos.";

            if (trimmed.Length > NumberMaxLength)
                return $"El número no puede exceder los {NumberMaxLength} dígitos.";

            if (trimmed.All(c => c == '0'))
                return "El número debe ser mayor a cero.";

            return null;
        }

        /// <summary>La fecha no puede ser futura (en hora de Perú) ni de antes del 2000, que solo puede ser un año mal escrito.</summary>
        public static string? IssueDateError(DateOnly? issueDate)
        {
            if (issueDate is null)
                return "La fecha de emisión es requerida.";

            if (issueDate > PeruCalendar.Today(DateTime.UtcNow))
                return "La fecha de emisión no puede ser mayor a la fecha actual.";

            if (issueDate < MinIssueDate)
                return "La fecha de emisión es demasiado antigua. Revisa el año.";

            return null;
        }

        public static string? CurrencyError(Currency? currency) =>
            currency switch
            {
                null => "La moneda es requerida.",
                { } value when !Enum.IsDefined(value) => "La moneda es inválida.",
                _ => null
            };

        /// <summary>Solo en moneda extranjera, mayor a cero y con hasta 6 decimales (los que guarda la base).</summary>
        public static string? ExchangeRateError(Currency? currency, decimal? exchangeRate) =>
            (currency, exchangeRate) switch
            {
                (Currency.PEN, not null) => "El tipo de cambio no aplica para soles.",
                (not null and not Currency.PEN, null) => "El tipo de cambio es requerido para moneda extranjera.",
                (_, <= 0) => "El tipo de cambio debe ser mayor a cero.",
                (_, > ExchangeRateMax) => "El tipo de cambio es demasiado grande. Revisa que esté bien escrito.",
                (_, { } rate) when Math.Round(rate, ExchangeRateDecimals) != rate =>
                    $"El tipo de cambio puede tener hasta {ExchangeRateDecimals} decimales.",
                _ => null
            };

        public static string? InvoicePriceTypeError(InvoicePriceType? invoicePriceType) =>
            invoicePriceType switch
            {
                null => "El tipo de precio es requerido.",
                { } value when !Enum.IsDefined(value) => "El tipo de precio es inválido.",
                _ => null
            };

        /// <summary>El largo se mide sobre el motivo ya normalizado (sin espacios de sobra).</summary>
        public static string? CancellationReasonError(string? cancellationReason)
        {
            if (string.IsNullOrWhiteSpace(cancellationReason))
                return "El motivo de anulación es requerido.";

            if (TextNormalizer.CollapseSpaces(cancellationReason).Length > CancellationReasonMaxLength)
                return $"El motivo de anulación no puede exceder los {CancellationReasonMaxLength} caracteres.";

            return null;
        }

        private static void Throw(string? error)
        {
            if (error is not null)
                throw new DomainException(error);
        }
    }
}

