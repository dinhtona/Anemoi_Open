using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetEmployeeShiftAssignments;

public sealed record GetEmployeeShiftAssignmentsQuery(
    EmployeeId EmployeeId = null,
    ShiftTemplateId ShiftTemplateId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string Status = null,
    int Page = 1,
    int PageSize = 50,
    string SortBy = null,
    string SortDirection = "asc") : IQuery<PaginationResponse<EmployeeShiftAssignmentResponse>>;
