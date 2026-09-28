using ERP.Api.Controllers.Audit.Requests;
using ERP.Api.Extensions;
using ERP.Application.Common.Pagination;
using ERP.Application.Features.Audit.ListAuditActions;
using ERP.Application.Features.Audit.ListAuditEntityTypes;
using ERP.Application.Features.Audit.ListAuditEntries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Audit
{
    /// <summary>
    /// Historial de cambios. Con EntityType y EntityId devuelve el historial de un registro;
    /// sin ellos, el de todo el sistema.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    [Route("api/audit")]
    public sealed class AuditController : ControllerBase
    {
        private readonly IListAuditEntriesUseCase _listAuditEntriesUseCase;
        private readonly IListAuditEntityTypesUseCase _listAuditEntityTypesUseCase;
        private readonly IListAuditActionsUseCase _listAuditActionsUseCase;

        public AuditController(
            IListAuditEntriesUseCase listAuditEntriesUseCase,
            IListAuditEntityTypesUseCase listAuditEntityTypesUseCase,
            IListAuditActionsUseCase listAuditActionsUseCase
            )
        {
            _listAuditEntriesUseCase = listAuditEntriesUseCase;
            _listAuditEntityTypesUseCase = listAuditEntityTypesUseCase;
            _listAuditActionsUseCase = listAuditActionsUseCase;
        }

        [HttpGet]
        [ProducesResponseType<PagedResult<AuditEntryDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery]ListAuditEntriesRequest listAuditEntriesRequest)
        {
            var result = await _listAuditEntriesUseCase.ExecuteAsync(listAuditEntriesRequest.ToDto());
            return result.ToActionResult(StatusCodes.Status200OK);
        }

        [HttpGet("entity-types")]
        [ProducesResponseType<IReadOnlyCollection<ListAuditEntityTypesResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListEntityTypes() =>
            Ok(await _listAuditEntityTypesUseCase.ExecuteAsync());

        [HttpGet("actions")]
        [ProducesResponseType<IReadOnlyCollection<ListAuditActionsResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListActions() =>
            Ok(await _listAuditActionsUseCase.ExecuteAsync());
    }
}
