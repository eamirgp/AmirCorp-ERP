using ERP.Application.Common.Responses;
using ERP.Api.Controllers.Companies.Requests;
using ERP.Api.Extensions;
using ERP.Application.Features.Companies.Activate;
using ERP.Application.Features.Companies.CreateCompany;
using ERP.Application.Features.Companies.Deactivate;
using ERP.Application.Features.Companies.GetCompany;
using ERP.Application.Features.Companies.ListCompanies;
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

        public CompanyController(
            ICreateCompanyUseCase createCompanyUseCase,
            IListCompaniesUseCase listCompaniesUseCase,
            IActivateCompanyUseCase activateCompanyUseCase,
            IDeactivateCompanyUseCase deactivateCompanyUseCase,
            IGetCompanyUseCase getCompanyUseCase,
            IUpdateCompanyUseCase updateCompanyUseCase
            )
        {
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
        public async Task<IActionResult> List() =>
            Ok(await _listCompaniesUseCase.ExecuteAsync());

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
            var response = await _getCompanyUseCase.ExecuteAsync(new GetCompanyDto(id));
            return response is null ? NotFound() : Ok(response);
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
