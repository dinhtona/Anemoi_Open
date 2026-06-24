using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Overtime;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyOvertimeRequests;

public sealed class GetMyOvertimeRequestsHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<OvertimeRequest> overtimeRequestRepository,
    EssMapper mapper,
    IWorkflowQueryService workflowQueryService)
    : IQueryHandler<GetMyOvertimeRequestsQuery, OneOf<IReadOnlyCollection<EssOvertimeRequestResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EssOvertimeRequestResponse>, ErrorDetailResponse>> Handle(
        GetMyOvertimeRequestsQuery request, CancellationToken cancellationToken)
    {
        Employee employee = null;
        if (Guid.TryParse(request.UserId, out var identityUserId))
            employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.IdentityUserId == identityUserId, null, cancellationToken);

        if (employee is null && !string.IsNullOrEmpty(request.Email))
        {
            var searchEmail = request.Email.ToLower();
            employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.WorkEmail != null && x.WorkEmail.ToLower() == searchEmail, null, cancellationToken);
        }

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var requests = await overtimeRequestRepository.GetManyByConditionAsync(
            x => x.EmployeeId == employee.Id,
            query => query.OrderByDescending(r => r.CreatedAt),
            cancellationToken);

        var entityIds = requests.Select(r => r.Id.Value).ToList();
        var workflowSummaries = await workflowQueryService.GetWorkflowSummariesAsync(
            WorkflowConstants.TargetEntityTypes.OvertimeRequest, entityIds, cancellationToken);

        var responses = new List<EssOvertimeRequestResponse>();
        foreach (var overtimeRequest in requests)
        {
            var mapped = mapper.ToEssOvertimeRequestResponse(overtimeRequest);
            workflowSummaries.TryGetValue(mapped.Id, out var summary);
            responses.Add(new EssOvertimeRequestResponse
            {
                Id = mapped.Id,
                OvertimeDate = mapped.OvertimeDate,
                StartTime = mapped.StartTime,
                EndTime = mapped.EndTime,
                DurationHours = mapped.DurationHours,
                Reason = mapped.Reason,
                Status = mapped.Status,
                CreatedAt = mapped.CreatedAt,
                CurrentApproverName = summary?.CurrentApproverName,
                CurrentStepName = summary?.CurrentStepName,
                WorkflowStatus = summary?.WorkflowStatus
            });
        }

        return responses;
    }
}
