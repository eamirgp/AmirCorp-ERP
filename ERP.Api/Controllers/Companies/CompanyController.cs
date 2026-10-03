using ERP.Api.Common;
using Microsoft.AspNetCore.RateLimiting;
using ERP.Application.Common.Responses;
using ERP.Api.Controllers.Companies.Requests;
using ERP.Api.Extensions;
using ERP.Application.Features.Companies.Activate;
using ERP.Application.Features.Companies.CreateCompany;
using ERP.Application.Features.Companies.Deactivate;
using ERP.Application.Features.Companies.GetCompany;
using ERP.Application.Common.Lookup;
using ERP.Application.Features.Companies.ListCompanies;
using ERP.Application.Features.Companies.LookupRuc;
using ERP.Application.Features.Companies.UpdateCompany;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Companies
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    [Route("api/companies")]
    public sealed class CompanyController : ControllerBase
    {
        private readonly ICreateCompanyUseCase _createCompanyUseCase;
        private readonly IListCompaniesUseCase _listCompaniesUseCase;
        private readonly IActivateCompanyUseCase _activateCompanyUseCase;
        private readonly IDeactivateCompanyUseCase _deactivateCompanyUseCase;
        private readonly IGetCompanyUseCase _getCompanyUseCase;
        private readonly IUpdateCompanyUseCase _updateCompanyUseCase;
        private readonly ILookupCompanyRucUseCase _lookupCompanyRucUseCase;

        public CompanyController(
            ICreateCompanyUseCase createCompanyUseCase,
            IListCompaniesUseCase listCompaniesUseCase,
            IActivateCompanyUseCase activateCompanyUseCase,
            IDeactivateCompanyUseCase deactivateCompanyUseCase,
            IGetCompanyUseCase getCompanyUseCase,
            IUpdateCompanyUseCase updateCompanyUseCase,
            ILookupCompanyRucUseCase lookupCompanyRucUseCase
            )
        {
            _lookupCompanyRucUseCase = lookupCompanyRucUseCase;
            _createCompanyUseCase = createCompanyUseCase;
            _listCompaniesUseCase = listCompaniesUseCase;
            _activateCompanyUseCase = activateCompanyUseCase;
            _deactivateCompanyUseCase = deactivateCompanyUseCase;
            _getCompanyUseCase = getCompanyUseCase;
            _updateCompanyUseCase = updateCompanyUseCase;
        }

        [HttpPost]
        [ProducesResponseType<CreatedResponseDto>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody]CreateCompanyRequest createCompanyRequest)
        {
            var errors = createCompanyRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _createCompanyUseCase.ExecuteAsync(createCompanyRequest.ToDto());
            return result.ToActionResult(StatusCodes.Status201Created);
        }

        [HttpGet]
        [ProducesResponseType<IReadOnlyCollection<ListCompaniesResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] ListFilterRequest filter) =>
            Ok(await _listCompaniesUseCase.ExecuteAsync(filter.ToDto()));

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _activateCompanyUseCase.ExecuteAsync(new ActivateCompanyDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _deactivateCompanyUseCase.ExecuteAsync(new DeactivateCompanyDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType<GetCompanyResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _getCompanyUseCase.ExecuteAsync(new GetCompanyDto(id));
            return result.ToActionResult(StatusCodes.Status200OK);
        }

        /// <summary>
        /// Busca el RUC en SUNAT (razón social, estado, condición y dirección) para llenar el formulario. 503 si la
        /// consulta no está configurada o el servicio no responde; el formulario sigue funcionando a mano.
        /// </summary>
        [HttpGet("ruc-lookup")]
        [EnableRateLimiting(RateLimits.ExternalLookup)]
        [ProducesResponseType<LookupDocumentResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> LookupRuc([FromQuery] string? ruc, [FromQuery] Guid? companyId, CancellationToken ct)
        {
            // companyId: la empresa que se está editando; así no se avisa "ya registrada" por ella misma.
            var result = await _lookupCompanyRucUseCase.ExecuteAsync(ruc ?? "", companyId, ct);
            return result.ToActionResult(StatusCodes.Status200OK);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody]UpdateCompanyRequest updateCompanyRequest)
        {
            var errors = updateCompanyRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _updateCompanyUseCase.ExecuteAsync(updateCompanyRequest.ToDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }
    }
}
