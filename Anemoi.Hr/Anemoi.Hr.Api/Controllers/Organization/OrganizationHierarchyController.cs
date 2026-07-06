using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Services;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Organization;

[ApiController]
[Route("api/hr/organization/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class OrganizationHierarchyController(IOrganizationService organizationService) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.OrganizationView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<Domain.Organization.OrganizationNode>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationTree(CancellationToken cancellationToken)
    {
        var tree = await organizationService.GetOrganizationTreeAsync(cancellationToken);
        return Ok(tree);
    }

    [HttpGet("{employeeId}")]
    [HasPermission(HrPermissions.OrganizationView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<Domain.Organization.ReportingRelationship>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportingChain([FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var chain = await organizationService.GetReportingChainAsync(employeeId, cancellationToken);
        return Ok(chain);
    }

    [HttpPut]
    [HasPermission(HrPermissions.OrganizationManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateManager(
        [FromBody] UpdateManagerRequest request,
        CancellationToken cancellationToken)
    {
        var res = await organizationService.UpdateManagerAsync(
            request.EmployeeId, request.ManagerId, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.WorkflowOverride)]
    [ProducesResponseType(typeof(IReadOnlyCollection<Application.Models.WorkflowApproverCandidate>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApproversPreview(
        [FromQuery] EmployeeId employeeId, [FromQuery] string entityType,
        CancellationToken cancellationToken)
    {
        var preview = await organizationService.GetApproversPreviewAsync(employeeId, entityType, cancellationToken);
        return Ok(preview);
    }

    [HttpGet]
    [HasPermission(HrPermissions.WorkflowOverride)]
    [ProducesResponseType(typeof(IReadOnlyCollection<Application.Models.WorkflowApproverCandidate>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkflowRoutePreview(
        [FromQuery] EmployeeId employeeId, [FromQuery] string entityType,
        CancellationToken cancellationToken)
    {
        var preview = await organizationService.GetWorkflowRoutePreviewAsync(employeeId, entityType, cancellationToken);
        return Ok(preview);
    }
}

public sealed record UpdateManagerRequest(EmployeeId EmployeeId, EmployeeId? ManagerId);
