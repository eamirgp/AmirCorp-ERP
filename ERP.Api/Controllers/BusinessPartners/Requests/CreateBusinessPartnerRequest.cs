using ERP.Application.Features.Partners.CreateBusinessPartner;
using ERP.Domain.Catalogs;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;
using DocType = ERP.Domain.Partners.Enums.IdentityDocumentType;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    public sealed record CreateBusinessPartnerRequest(
        DocType? IdentityDocumentType,
        string? DocumentNumber,
        Country? Country,
        string? Name,
        bool? IsClient,
        bool? IsSupplier
        )
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (IdentityDocumentType is null)
                errors.Add("El tipo de documento es requerido.");

            if (IdentityDocumentType is not null && !Enum.IsDefined(IdentityDocumentType.Value))
                errors.Add("El tipo de documento es inválido.");

            if (string.IsNullOrWhiteSpace(DocumentNumber))
                errors.Add("El número de documento es requerido.");

            if (IdentityDocumentType == DocType.Dni && (!string.IsNullOrWhiteSpace(DocumentNumber) && (DocumentNumber.Length != IdentityDocumentTypeExtensions.DniLength || !DocumentNumber.All(char.IsDigit))))
                errors.Add($"El DNI debe tener {IdentityDocumentTypeExtensions.DniLength} dígitos.");

            if (IdentityDocumentType == DocType.Ruc && (!string.IsNullOrWhiteSpace(DocumentNumber) && (DocumentNumber.Length != IdentityDocumentTypeExtensions.RucLength || !DocumentNumber.All(char.IsDigit))))
                errors.Add($"El RUC debe tener {IdentityDocumentTypeExtensions.RucLength} dígitos.");

            if (IdentityDocumentType == DocType.TributarioExtranjero && (!string.IsNullOrWhiteSpace(DocumentNumber) && DocumentNumber.Length > IdentityDocumentTypeExtensions.ForeignMaxLength))
                errors.Add($"El documento tributario extranjero no puede exceder los {IdentityDocumentTypeExtensions.ForeignMaxLength} caracteres.");

            if (Country is null)
                errors.Add("El país es requerido.");

            if (Country is not null && !Enum.IsDefined(Country.Value))
                errors.Add("El país es inválido.");

            if (IdentityDocumentType is not null && Country is not null)
            {
                if (IdentityDocumentType.Value.RequiresPeruvianCountry && !Country.Value.IsPeru)
                    errors.Add("Para DNI o RUC el país debe ser Perú.");

                if (!IdentityDocumentType.Value.RequiresPeruvianCountry && Country.Value.IsPeru)
                    errors.Add("Para documento tributario extranjero corresponde a un socio no domiciliado.");
            }

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("El nombre es requerido.");

            if (!string.IsNullOrWhiteSpace(Name) && Name.Length > BusinessPartner.NameMaxLength)
                errors.Add($"El nombre no puede exceder los {BusinessPartner.NameMaxLength} caracteres.");

            if (IsClient is null)
                errors.Add("Debe indicar si el socio es cliente.");

            if (IsSupplier is null)
                errors.Add("Debe indicar si el socio es proveedor.");

            if (IsClient is false && IsSupplier is false)
                errors.Add("El socio debe ser cliente, proveedor o ambos.");

            if (IsSupplier is true && IdentityDocumentType is not null && !IdentityDocumentType.Value.CanIssueTaxDocuments)
                errors.Add("Un proveedor debe tener RUC o ser tributario extranjero para emitir comprobantes válidos.");

            return errors;
        }

        public CreateBusinessPartnerDto ToDto() =>
            new(IdentityDocumentType!.Value, DocumentNumber!, Country!.Value, Name!, IsClient!.Value, IsSupplier!.Value);
    }
}
