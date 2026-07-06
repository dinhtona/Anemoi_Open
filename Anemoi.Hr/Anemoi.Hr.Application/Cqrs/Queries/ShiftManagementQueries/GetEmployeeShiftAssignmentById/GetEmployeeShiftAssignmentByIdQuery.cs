using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetEmployeeShiftAssignmentById;

public sealed record GetEmployeeShiftAssignmentByIdQuery(
    EmployeeShiftAssignmentId Id) : IQuery<EmployeeShiftAssignmentResponse>;
