using ERP.Api.Common;
using ERP.Application.Common.Pagination;
using ERP.Application.Common.Responses;
using ERP.Api.Controllers.BusinessPartners.Requests;
using ERP.Api.Extensions;
using ERP.Application.Features.Partners.ActivateBusinessPartner;
using ERP.Application.Features.Partners.CreateBusinessPartner;
using ERP.Application.Features.Partners.DeactivateBusinessPartner;
using ERP.Application.Features.Partners.GetBusinessPartner;
using ERP.Application.Features.Partners.ListBusinessPartners;
using ERP.Application.Features.Partners.ListIdentityDocumentTypes;
using ERP.Application.Features.Partners.LookupRuc;
using ERP.Application.Features.Partners.UpdateBusinessPartner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.BusinessPartners
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    [Route("api/partners")]
    public sealed class BusinessPartnerController : ControllerBase
    {
        private readonly ICreateBusinessPartnerUseCase _createBusinessPartnerUseCase;
        private readonly IListIdentityDocumentTypesUseCase _listIdentityDocumentTypesUseCase;
        private readonly IListBusinessPartnersUseCase _listBusinessPartnersUseCase;
        private readonly IActivateBusinessPartnerUseCase _activateBusinessPartnerUseCase;
        private readonly IDeactivateBusinessPartnerUseCase _deactivateBusinessPartnerUseCase;
        private readonly IUpdateBusinessPartnerUseCase _updateBusinessPartnerUseCase;
        private readonly IGetBusinessPartnerUseCase _getBusinessPartnerUseCase;
        private readonly ILookupRucUseCase _lookupRucUseCase;

        public BusinessPartnerController(
            ICreateBusinessPartnerUseCase createBusinessPartnerUseCase,
            IListIdentityDocumentTypesUseCase listIdentityDocumentTypesUseCase,
            IListBusinessPartnersUseCase listBusinessPartnersUseCase,
            IActivateBusinessPartnerUseCase activateBusinessPartnerUseCase,
            IDeactivateBusinessPartnerUseCase deactivateBusinessPartnerUseCase,
            IUpdateBusinessPartnerUseCase updateBusinessPartnerUseCase,
            IGetBusinessPartnerUseCase getBusinessPartnerUseCase,
            ILookupRucUseCase lookupRucUseCase
            )
        {
            _lookupRucUseCase = lookupRucUseCase;
            _createBusinessPartnerUseCase = createBusinessPartnerUseCase;
            _listIdentityDocumentTypesUseCase = listIdentityDocumentTypesUseCase;
            _listBusinessPartnersUseCase = listBusinessPartnersUseCase;
            _activateBusinessPartnerUseCase = activateBusinessPartnerUseCase;
            _deactivateBusinessPartnerUseCase = deactivateBusinessPartnerUseCase;
            _updateBusinessPartnerUseCase = updateBusinessPartnerUseCase;
            _getBusinessPartnerUseCase = getBusinessPartnerUseCase;
        }

        [HttpPost]
        [ProducesResponseType<CreatedResponseDto>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody]CreateBusinessPartnerRequest createBusinessPartnerRequest)
        {
            var errors = createBusinessPartnerRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _createBusinessPartnerUseCase.ExecuteAsync(createBusinessPartnerRequest.ToDto());
            return result.ToActionResult(StatusCodes.Status201Created);
        }

        /// <summary>
        /// Busca un RUC en SUNAT (razón social, estado, condición y dirección) para llenar el formulario.
        /// 503 si la consulta no está configurada o el servicio no responde; el formulario sigue funcionando a mano.
        /// </summary>
        [HttpGet("ruc-lookup/{ruc}")]
        [ProducesResponseType<LookupRucResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> LookupRuc(string ruc, CancellationToken ct)
        {
            var result = await _lookupRucUseCase.ExecuteAsync(ruc, ct);
            return result.ToActionResult(StatusCodes.Status200OK);
        }

        [HttpGet("identity-document-types")]
        [ProducesResponseType<IReadOnlyCollection<ListIdentityDocumentTypesResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListIdentityDocumentTypes() =>
            Ok(await _listIdentityDocumentTypesUseCase.ExecuteAsync());

        [HttpGet]
        [ProducesResponseType<SortedPagedResult<ListBusinessPartnersResponseDto, BusinessPartnerSortBy>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery]ListBusinessPartnersRequest listBusinessPartnersRequest)
        {
            var response = await _listBusinessPartnersUseCase.ExecuteAsync(listBusinessPartnersRequest.ToDto());
            return Ok(response);
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _activateBusinessPartnerUseCase.ExecuteAsync(new ActivateBusinessPartnerDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _deactivateBusinessPartnerUseCase.ExecuteAsync(new DeactivateBusinessPartnerDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody]UpdateBusinessPartnerRequest updateBusinessPartnerRequest)
        {
            var errors = updateBusinessPartnerRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _updateBusinessPartnerUseCase.ExecuteAsync(updateBusinessPartnerRequest.ToDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType<GetBusinessPartnerResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await _getBusinessPartnerUseCase.ExecuteAsync(new GetBusinessPartnerDto(id));
            return response is null
                ? NotFound(new ErrorResponse(["El cliente o proveedor no existe."]))
                : Ok(response);
        }
    }
}
