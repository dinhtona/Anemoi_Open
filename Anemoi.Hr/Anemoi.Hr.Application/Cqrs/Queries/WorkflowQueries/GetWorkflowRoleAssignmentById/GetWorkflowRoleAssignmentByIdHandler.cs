using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowRoleAssignmentById;

public sealed class GetWorkflowRoleAssignmentByIdHandler(
    ISqlRepository<WorkflowRoleAssignment> repository,
    WorkflowRoleAssignmentMapper mapper)
    : IQueryHandler<GetWorkflowRoleAssignmentByIdQuery, OneOf<WorkflowRoleAssignmentResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowRoleAssignmentResponse, ErrorDetailResponse>> Handle(
        GetWorkflowRoleAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assignment = await repository.GetQueryable()
            .Include(x => x.Employee)
            .ThenInclude(x => x.PrimaryDepartment)
            .Include(x => x.Employee)
            .ThenInclude(x => x.PrimaryPosition)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (assignment is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowRoleAssignmentNotFound);

        return mapper.ToResponse(assignment);
    }
}
