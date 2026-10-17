using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Organization;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetOrganizationTree;

public sealed class GetOrganizationTreeHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository,
    OrganizationMapper mapper)
    : IQueryHandler<GetOrganizationTreeQuery, OneOf<List<OrganizationNodeResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<List<OrganizationNodeResponse>, ErrorDetailResponse>> Handle(
        GetOrganizationTreeQuery request, CancellationToken cancellationToken)
    {
        var employees = await employeeRepository.GetQueryable()
            .Include(e => e.PrimaryDepartment)
            .Include(e => e.PrimaryPosition)
            .ToListAsync(cancellationToken);

        var departments = await departmentRepository.GetQueryable()
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        var employeeMap = employees.ToDictionary(e => e.Id);

        var childrenByManager = employees
            .Where(e => e.DirectManagerEmployeeId is not null)
            .GroupBy(e => e.DirectManagerEmployeeId!.Value)
            .ToDictionary(g => new EmployeeId(g.Key), g => g.OrderBy(e => e.FullName).ToList());

        var roots = employees
            .Where(e => e.DirectManagerEmployeeId is null)
            .OrderBy(e => e.FullName)
            .ToList();

        var tree = roots
            .Select(r => BuildNode(r, employeeMap, childrenByManager, departments))
            .Select(mapper.ToOrganizationNodeResponse)
            .ToList();

        return tree;
    }

    private static OrganizationNode BuildNode(Employee employee,
        Dictionary<EmployeeId, Employee> employeeMap,
        Dictionary<EmployeeId, List<Employee>> childrenByManager,
        Dictionary<DepartmentId, Department> departments)
    {
        var dept = employee.PrimaryDepartmentId is not null
            ? departments.GetValueOrDefault(employee.PrimaryDepartmentId)
            : null;

        var manager = employee.DirectManagerEmployeeId is not null
            ? employeeMap.GetValueOrDefault(employee.DirectManagerEmployeeId)
            : null;

        var directReports = childrenByManager.GetValueOrDefault(employee.Id)
            ?? new List<Employee>();

        return new OrganizationNode(
            employee.Id,
            employee.FullName,
            employee.EmployeeCode,
            employee.DirectManagerEmployeeId,
            manager?.FullName,
            employee.PrimaryDepartmentId,
            dept?.Name ?? string.Empty,
            employee.PrimaryPosition?.Name ?? string.Empty,
            employee.GradeCode,
            directReports.Select(r => BuildNode(r, employeeMap, childrenByManager, departments)).ToList());
    }
}
