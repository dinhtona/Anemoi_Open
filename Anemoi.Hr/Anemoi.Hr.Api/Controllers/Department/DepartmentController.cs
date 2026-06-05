using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.DepartmentQueries.GetDepartments;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Department;

[ApiController]
[Route("api/hr/department/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class DepartmentController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.DepartmentView)]
    [ProducesResponseType(typeof(PaginationResponse<DepartmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments([FromQuery] GetDepartmentsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }
}
