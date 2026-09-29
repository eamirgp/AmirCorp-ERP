using ERP.Application.Features.Catalogs.ListCountries;
using ERP.Application.Features.Catalogs.ListCurrencies;
using ERP.Application.Features.Catalogs.ListIgvAffectations;
using ERP.Application.Features.Catalogs.ListInvoicePriceTypes;
using ERP.Application.Features.Catalogs.ListTaxDocumentTypes;
using ERP.Application.Features.Catalogs.ListUnitsOfMeasure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Catalogs
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    [Route("api/catalogs")]
    public sealed class CatalogController : ControllerBase
    {
        private readonly IListCountriesUseCase _listCountriesUseCase;
        private readonly IListCurrenciesUseCase _listCurrenciesUseCase;
        private readonly IListIgvAffectationsUseCase _listIgvAffectationsUseCase;
        private readonly IListTaxDocumentTypesUseCase _listTaxDocumentTypesUseCase;
        private readonly IListUnitsOfMeasureUseCase _listUnitsOfMeasureUseCase;
        private readonly IListInvoicePriceTypesUseCase _listInvoicePriceTypesUseCase;

        public CatalogController(
            IListCountriesUseCase listCountriesUseCase,
            IListCurrenciesUseCase listCurrenciesUseCase,
            IListIgvAffectationsUseCase listIgvAffectationsUseCase,
            IListTaxDocumentTypesUseCase listTaxDocumentTypesUseCase,
            IListUnitsOfMeasureUseCase listUnitsOfMeasureUseCase,
            IListInvoicePriceTypesUseCase listInvoicePriceTypesUseCase
            )
        {
            _listCountriesUseCase = listCountriesUseCase;
            _listCurrenciesUseCase = listCurrenciesUseCase;
            _listIgvAffectationsUseCase = listIgvAffectationsUseCase;
            _listTaxDocumentTypesUseCase = listTaxDocumentTypesUseCase;
            _listUnitsOfMeasureUseCase = listUnitsOfMeasureUseCase;
            _listInvoicePriceTypesUseCase = listInvoicePriceTypesUseCase;
        }

        /// <summary>Países para un documento extranjero (sin Perú).</summary>
        [HttpGet("countries")]
        [ProducesResponseType<IReadOnlyCollection<ListCountriesResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListCountries() =>
            Ok(await _listCountriesUseCase.ExecuteAsync());

        [HttpGet("currencies")]
        [ProducesResponseType<IReadOnlyCollection<ListCurrenciesResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListCurrencies() =>
            Ok(await _listCurrenciesUseCase.ExecuteAsync());

        [HttpGet("igv-affectations")]
        [ProducesResponseType<IReadOnlyCollection<ListIgvAffectationsResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListIgvAffectations() =>
            Ok(await _listIgvAffectationsUseCase.ExecuteAsync());

        [HttpGet("tax-document-types")]
        [ProducesResponseType<IReadOnlyCollection<ListTaxDocumentTypesResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListTaxDocumentTypes() =>
            Ok(await _listTaxDocumentTypesUseCase.ExecuteAsync());

        [HttpGet("units-of-measure")]
        [ProducesResponseType<IReadOnlyCollection<ListUnitsOfMeasureResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListUnitsOfMeasure() =>
            Ok(await _listUnitsOfMeasureUseCase.ExecuteAsync());

        [HttpGet("invoice-price-types")]
        [ProducesResponseType<IReadOnlyCollection<ListInvoicePriceTypesResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListInvoicePriceTypes() =>
            Ok(await _listInvoicePriceTypesUseCase.ExecuteAsync());
    }
}
