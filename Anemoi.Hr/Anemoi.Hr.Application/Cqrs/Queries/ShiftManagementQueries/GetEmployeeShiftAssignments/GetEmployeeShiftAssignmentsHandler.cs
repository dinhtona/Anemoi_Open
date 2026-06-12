using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.ShiftManagement;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetEmployeeShiftAssignments;

public sealed class GetEmployeeShiftAssignmentsHandler(
    ISqlRepository<EmployeeShiftAssignment> assignmentRepository,
    ShiftManagementMapper mapper)
    : IQueryHandler<GetEmployeeShiftAssignmentsQuery, PaginationResponse<EmployeeShiftAssignmentResponse>>
{
    public async Task<PaginationResponse<EmployeeShiftAssignmentResponse>> Handle(
        GetEmployeeShiftAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<EmployeeShiftAssignment> query = assignmentRepository.GetQueryable()
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.ShiftTemplate);

        if (request.EmployeeId is not null)
            query = query.Where(x => x.EmployeeId == request.EmployeeId);

        if (request.ShiftTemplateId is not null)
            query = query.Where(x => x.ShiftTemplateId == request.ShiftTemplateId);

        if (request.FromDate.HasValue)
            query = query.Where(x => x.WorkDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(x => x.WorkDate <= request.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.Status == request.Status);

        query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
        {
            ("workdate", "desc") => query.OrderByDescending(x => x.WorkDate),
            ("workdate", "asc") => query.OrderBy(x => x.WorkDate),
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),
            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            ("employeename", "desc") => query.OrderByDescending(x => x.Employee.FullName),
            ("employeename", "asc") => query.OrderBy(x => x.Employee.FullName),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<EmployeeShiftAssignmentResponse>(
            rows.Select(mapper.ToEmployeeShiftAssignmentResponse).ToList(),
            totalRecords);
    }
}
