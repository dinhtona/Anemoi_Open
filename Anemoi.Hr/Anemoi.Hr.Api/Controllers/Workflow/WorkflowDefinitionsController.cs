using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ActivateWorkflowDefinition;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CreateWorkflowDefinition;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.DeactivateWorkflowDefinition;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.UpdateWorkflowDefinition;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitionById;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Workflow;

[ApiController]
[Route("api/hr/workflows/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class WorkflowDefinitionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.WorkflowView)]
    [ProducesResponseType(typeof(PaginationResponse<WorkflowDefinitionResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<WorkflowDefinitionResponse>> GetWorkflowDefinitions(
        [FromQuery] GetWorkflowDefinitionsQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.WorkflowView)]
    [ProducesResponseType(typeof(WorkflowDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkflowDefinitionById(
        [FromRoute] string id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetWorkflowDefinitionByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateWorkflowDefinition(
        [FromBody] CreateWorkflowDefinitionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateWorkflowDefinition(
        [FromBody] UpdateWorkflowDefinitionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateWorkflowDefinition(
        [FromBody] ActivateWorkflowDefinitionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateWorkflowDefinition(
        [FromBody] DeactivateWorkflowDefinitionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
