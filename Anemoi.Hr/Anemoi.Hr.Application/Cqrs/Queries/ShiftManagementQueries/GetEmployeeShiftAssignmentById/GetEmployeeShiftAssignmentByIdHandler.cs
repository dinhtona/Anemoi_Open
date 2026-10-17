using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.ShiftManagement;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetEmployeeShiftAssignmentById;

public sealed class GetEmployeeShiftAssignmentByIdHandler(
    ISqlRepository<EmployeeShiftAssignment> assignmentRepository,
    ShiftManagementMapper mapper)
    : IQueryHandler<GetEmployeeShiftAssignmentByIdQuery, OneOf<EmployeeShiftAssignmentResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeShiftAssignmentResponse, ErrorDetailResponse>> Handle(
        GetEmployeeShiftAssignmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetQueryable()
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.ShiftTemplate)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return assignment is null ? null : mapper.ToEmployeeShiftAssignmentResponse(assignment);
    }
}
