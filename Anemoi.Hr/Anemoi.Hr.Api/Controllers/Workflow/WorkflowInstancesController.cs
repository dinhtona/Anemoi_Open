using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ApproveWorkflowStep;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CancelWorkflow;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RejectWorkflowStep;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ReturnWorkflow;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.StartWorkflow;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstanceById;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstances;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetPendingApprovals;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Workflow;

[ApiController]
[Route("api/hr/workflows/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class WorkflowInstancesController(
    ISender sender,
    IWorkflowEngine workflowEngine) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartWorkflow(
        [FromBody] StartWorkflowCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { StartedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowApprove)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveWorkflowStep(
        [FromBody] ApproveWorkflowStepCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { PerformedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowApprove)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectWorkflowStep(
        [FromBody] RejectWorkflowStepCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { PerformedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowApprove)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelWorkflow(
        [FromBody] CancelWorkflowCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { PerformedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowApprove)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReturnWorkflow(
        [FromBody] ReturnWorkflowCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { PerformedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.WorkflowView)]
    [ProducesResponseType(typeof(PaginationResponse<WorkflowInstanceResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<WorkflowInstanceResponse>> GetWorkflowInstances(
        [FromQuery] GetWorkflowInstancesQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.WorkflowView)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkflowInstanceById(
        [FromRoute] string id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetWorkflowInstanceByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.WorkflowApprove)]
    [ProducesResponseType(typeof(PaginationResponse<WorkflowInstanceResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<WorkflowInstanceResponse>> GetPendingApprovals(
        [FromQuery] GetPendingApprovalsQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query with { UserId = HttpContext.GetUserId() }, cancellationToken);
    }

    [HttpGet("{id}/approvers")]
    [HasPermission(HrPermissions.WorkflowApprove)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentApprovers(
        [FromRoute] string id, CancellationToken cancellationToken)
    {
        var approvers = await workflowEngine.GetCurrentApproversAsync(
            new WorkflowInstanceId(Guid.Parse(id)), cancellationToken);
        return Ok(approvers.Select(a => a.Value.ToString()).ToList());
    }
}
