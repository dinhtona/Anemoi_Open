using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstances;

public sealed class GetWorkflowInstancesHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowDefinition> definitionRepository,
    ISqlRepository<Employee> employeeRepository,
    WorkflowMapper mapper)
    : IQueryHandler<GetWorkflowInstancesQuery, PaginationResponse<WorkflowInstanceResponse>>
{
    public async Task<PaginationResponse<WorkflowInstanceResponse>> Handle(
        GetWorkflowInstancesQuery request, CancellationToken cancellationToken)
    {
        var query = instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(x => x.EntityType == request.EntityType);
        if (!string.IsNullOrEmpty(request.WorkflowDefinitionId))
        {
            var defId = new WorkflowDefinitionId(Guid.Parse(request.WorkflowDefinitionId));
            query = query.Where(x => x.WorkflowDefinitionId == defId);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, string> defNames = [];
        Dictionary<Guid, string> empNames = [];
        if (items.Count != 0)
        {
            var defIds = items
                .Where(x => x.WorkflowDefinitionId is not null)
                .Select(x => x.WorkflowDefinitionId!)
                .Distinct()
                .ToList();
            if (defIds.Count > 0)
            {
                var filteredDefs = await definitionRepository.GetQueryable()
                    .Where(d => defIds.Contains(d.Id))
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                defNames = filteredDefs.ToDictionary(d => d.Id.Value, d => d.Name);
            }

            var employeeIds = new HashSet<EmployeeId>();
            foreach (var item in items)
            {
                employeeIds.Add(item.RequesterEmployeeId);
                var currentStep = item.Steps.FirstOrDefault(s => s.Sequence == item.CurrentStep);
                if (currentStep?.ApproverEmployeeId is not null)
                    employeeIds.Add(currentStep.ApproverEmployeeId);
            }

            if (employeeIds.Count > 0)
            {
                var filteredEmployees = await employeeRepository.GetQueryable()
                    .Where(e => employeeIds.Contains(e.Id))
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                empNames = filteredEmployees.ToDictionary(e => e.Id.Value, e => e.FullName);
            }
        }

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        return new PaginationResponse<WorkflowInstanceResponse>(
            mapper.ToResponses(items, defNames, empNames).ToList(), total);
    }
}
