using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Organization;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetReportingChain;

public sealed class GetReportingChainHandler(
    ISqlRepository<Employee> employeeRepository,
    OrganizationMapper mapper)
    : IQueryHandler<GetReportingChainQuery, OneOf<List<ReportingRelationshipResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<List<ReportingRelationshipResponse>, ErrorDetailResponse>> Handle(
        GetReportingChainQuery request, CancellationToken cancellationToken)
    {
        var allEmployees = await employeeRepository.GetQueryable()
            .Select(e => new
            {
                e.Id,
                e.FullName,
                e.DirectManagerEmployeeId
            })
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        if (!allEmployees.TryGetValue(request.EmployeeId, out var current))
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var chain = new List<ReportingRelationship>();

        var level = 0;
        var levels = new[] { HierarchyLevel.Employee, HierarchyLevel.DirectManager,
            HierarchyLevel.DepartmentManager, HierarchyLevel.HrManager };
        chain.Add(new ReportingRelationship(current.Id, current.FullName,
            current.DirectManagerEmployeeId,
            current.DirectManagerEmployeeId is not null && allEmployees.TryGetValue(current.DirectManagerEmployeeId, out var dm) ? dm.FullName : null,
            level < levels.Length ? levels[level] : $"Level{level}"));

        var visited = new HashSet<EmployeeId> { current.Id };
        var managerId = current.DirectManagerEmployeeId;
        level++;

        while (managerId is not null && !visited.Contains(managerId))
        {
            visited.Add(managerId);
            if (!allEmployees.TryGetValue(managerId, out var manager))
                break;

            chain.Add(new ReportingRelationship(manager.Id, manager.FullName,
                manager.DirectManagerEmployeeId,
                manager.DirectManagerEmployeeId is not null && allEmployees.TryGetValue(manager.DirectManagerEmployeeId, out var mgr) ? mgr.FullName : null,
                level < levels.Length ? levels[level] : $"Level{level}"));

            managerId = manager.DirectManagerEmployeeId;
            level++;
        }

        return chain.Select(mapper.ToReportingRelationshipResponse).ToList();
    }
}
