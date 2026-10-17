using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.OrganizationCommands.UpdateOrganizationManager;
using Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetApproversPreview;
using Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetOrganizationTree;
using Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetReportingChain;
using Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetWorkflowRoutePreview;
using Anemoi.Hr.Application.Models;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Organization;

[ApiController]
[Route("api/hr/organization/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class OrganizationHierarchyController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.OrganizationView)]
    [ProducesResponseType(typeof(List<OrganizationNodeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationTree(CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetOrganizationTreeQuery(), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{employeeId}")]
    [HasPermission(HrPermissions.OrganizationView)]
    [ProducesResponseType(typeof(List<ReportingRelationshipResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportingChain([FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetReportingChainQuery(employeeId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut]
    [HasPermission(HrPermissions.OrganizationManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateManager(
        [FromBody] UpdateManagerRequest request,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            new UpdateOrganizationManagerCommand(request.EmployeeId, request.ManagerId),
            cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.WorkflowOverride)]
    [ProducesResponseType(typeof(List<WorkflowApproverCandidate>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApproversPreview(
        [FromQuery] EmployeeId employeeId, [FromQuery] string entityType,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetApproversPreviewQuery(employeeId, entityType), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.WorkflowOverride)]
    [ProducesResponseType(typeof(List<WorkflowApproverCandidate>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkflowRoutePreview(
        [FromQuery] EmployeeId employeeId, [FromQuery] string entityType,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetWorkflowRoutePreviewQuery(employeeId, entityType), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}

public sealed record UpdateManagerRequest(EmployeeId EmployeeId, EmployeeId? ManagerId);
