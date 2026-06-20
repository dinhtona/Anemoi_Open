using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.AssignWorkflowRole;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RemoveWorkflowRoleAssignment;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowRoleAssignmentById;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowRoleAssignments;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Workflow;

[ApiController]
[Route("api/hr/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class WorkflowRoleAssignmentController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.WorkflowView)]
    [ProducesResponseType(typeof(PaginationResponse<WorkflowRoleAssignmentResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<WorkflowRoleAssignmentResponse>> GetWorkflowRoleAssignments(
        [FromQuery] GetWorkflowRoleAssignmentsQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.WorkflowView)]
    [ProducesResponseType(typeof(WorkflowRoleAssignmentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkflowRoleAssignmentById(
        [FromRoute] string id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            new GetWorkflowRoleAssignmentByIdQuery(new WorkflowRoleAssignmentId(Guid.Parse(id))),
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowRoleAssignmentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignWorkflowRole(
        [FromBody] AssignWorkflowRoleCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpDelete("{id}")]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveWorkflowRoleAssignment(
        [FromRoute] string id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            new RemoveWorkflowRoleAssignmentCommand(new WorkflowRoleAssignmentId(Guid.Parse(id))),
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
