using ERP.Api.Common;
using ERP.Application.Common.Lookup;
using ERP.Application.Common.Pagination;
using ERP.Application.Common.Responses;
using ERP.Api.Controllers.BusinessPartners.Requests;
using ERP.Api.Extensions;
using ERP.Application.Features.Partners.BlockBusinessPartnerRole;
using ERP.Application.Features.Partners.CreateBusinessPartner;
using ERP.Application.Features.Partners.GetBusinessPartner;
using ERP.Application.Features.Partners.ListBusinessPartners;
using ERP.Application.Features.Partners.ListIdentityDocumentTypes;
using ERP.Application.Features.Partners.LookupDocument;
using ERP.Application.Features.Partners.AddBusinessPartnerRole;
using ERP.Application.Features.Partners.FindBusinessPartnerByDocument;
using ERP.Domain.Partners.Enums;
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
        private readonly IBlockBusinessPartnerRoleUseCase _blockRoleUseCase;
        private readonly IUpdateBusinessPartnerUseCase _updateBusinessPartnerUseCase;
        private readonly IGetBusinessPartnerUseCase _getBusinessPartnerUseCase;
        private readonly ILookupDocumentUseCase _lookupDocumentUseCase;
        private readonly IAddBusinessPartnerRoleUseCase _addRoleUseCase;
        private readonly IFindBusinessPartnerByDocumentUseCase _findByDocumentUseCase;

        public BusinessPartnerController(
            ICreateBusinessPartnerUseCase createBusinessPartnerUseCase,
            IListIdentityDocumentTypesUseCase listIdentityDocumentTypesUseCase,
            IListBusinessPartnersUseCase listBusinessPartnersUseCase,
            IBlockBusinessPartnerRoleUseCase blockRoleUseCase,
            IUpdateBusinessPartnerUseCase updateBusinessPartnerUseCase,
            IGetBusinessPartnerUseCase getBusinessPartnerUseCase,
            ILookupDocumentUseCase lookupDocumentUseCase,
            IAddBusinessPartnerRoleUseCase addRoleUseCase,
            IFindBusinessPartnerByDocumentUseCase findByDocumentUseCase
            )
        {
            _lookupDocumentUseCase = lookupDocumentUseCase;
            _addRoleUseCase = addRoleUseCase;
            _findByDocumentUseCase = findByDocumentUseCase;
            _createBusinessPartnerUseCase = createBusinessPartnerUseCase;
            _listIdentityDocumentTypesUseCase = listIdentityDocumentTypesUseCase;
            _listBusinessPartnersUseCase = listBusinessPartnersUseCase;
            _blockRoleUseCase = blockRoleUseCase;
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
        /// Busca un RUC en SUNAT (razón social, estado, condición y dirección) o un DNI en RENIEC (apellidos y nombres)
        /// para llenar el formulario. 503 si la consulta no está configurada o el servicio no responde; el formulario
        /// sigue funcionando a mano.
        /// </summary>
        [HttpGet("document-lookup")]
        [ProducesResponseType<LookupDocumentResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> LookupDocument([FromQuery] IdentityDocumentType identityDocumentType, [FromQuery] string documentNumber, [FromQuery] Guid? partnerId, CancellationToken ct)
        {
            // partnerId: el registro que se está editando; así no se avisa "ya está registrado" por él mismo.
            var result = await _lookupDocumentUseCase.ExecuteAsync(identityDocumentType, documentNumber, partnerId, ct);
            return result.ToActionResult(StatusCodes.Status200OK);
        }

        /// <summary>
        /// Quién tiene ya ese documento (404 si nadie). Al crear, el formulario lo usa para ofrecer "agregarlo también
        /// a esta lista" en vez de crear un duplicado.
        /// </summary>
        [HttpGet("by-document")]
        [ProducesResponseType<FoundBusinessPartnerDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> FindByDocument([FromQuery] IdentityDocumentType identityDocumentType, [FromQuery] string documentNumber) =>
            await _findByDocumentUseCase.ExecuteAsync(identityDocumentType, documentNumber) is { } found
                ? Ok(found)
                : NotFound(new ErrorResponse(["No hay ningún cliente ni proveedor con ese documento."]));

        /// <summary>"Registrar también como cliente / proveedor": agrega el rol al mismo registro.</summary>
        [HttpPatch("{id:guid}/roles/{role}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddRole(Guid id, BusinessPartnerRole role)
        {
            var result = await _addRoleUseCase.ExecuteAsync(id, role);
            return result.ToActionResult(StatusCodes.Status204NoContent);
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

        /// <summary>
        /// Bloquea un rol: Supplier bloquea las compras y Client las ventas. El otro rol no cambia. El motivo es opcional.
        /// </summary>
        [HttpPatch("{id:guid}/roles/{role}/block")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> BlockRole(Guid id, BusinessPartnerRole role, [FromBody] BlockBusinessPartnerRoleRequest request)
        {
            var errors = request.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _blockRoleUseCase.ExecuteAsync(new BlockBusinessPartnerRoleDto(id, role, Blocked: true, request.Reason));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPatch("{id:guid}/roles/{role}/unblock")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UnblockRole(Guid id, BusinessPartnerRole role)
        {
            var result = await _blockRoleUseCase.ExecuteAsync(new BlockBusinessPartnerRoleDto(id, role, Blocked: false, Reason: null));
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
