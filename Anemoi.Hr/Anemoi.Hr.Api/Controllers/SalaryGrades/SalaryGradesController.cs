using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.SalaryGradeCommands.ActivateSalaryGrade;
using Anemoi.Hr.Application.Cqrs.Commands.SalaryGradeCommands.DeactivateSalaryGrade;
using Anemoi.Hr.Application.Cqrs.Commands.SalaryGradeCommands.UpdateSalaryGrade;
using Anemoi.Hr.Application.Cqrs.Queries.SalaryGradeQueries.GetSalaryGradeById;
using Anemoi.Hr.Application.Cqrs.Queries.SalaryGradeQueries.GetSalaryGrades;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.SalaryGrades;

[ApiController]
[Route("api/hr/salary-grades")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class SalaryGradesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.SalaryGradeView)]
    [ProducesResponseType(typeof(PaginationResponse<SalaryGradeResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<SalaryGradeResponse>> GetSalaryGrades(
        [FromQuery] GetSalaryGradesQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.SalaryGradeView)]
    [ProducesResponseType(typeof(SalaryGradeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalaryGradeById(
        [FromRoute] SalaryGradeId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetSalaryGradeByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.SalaryGradeManage)]
    [ProducesResponseType(typeof(SalaryGradeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSalaryGrade(
        [FromRoute] SalaryGradeId id,
        [FromBody] UpdateSalaryGradeCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/activate")]
    [HasPermission(HrPermissions.SalaryGradeManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateSalaryGrade(
        [FromRoute] SalaryGradeId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new ActivateSalaryGradeCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/deactivate")]
    [HasPermission(HrPermissions.SalaryGradeManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateSalaryGrade(
        [FromRoute] SalaryGradeId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new DeactivateSalaryGradeCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
