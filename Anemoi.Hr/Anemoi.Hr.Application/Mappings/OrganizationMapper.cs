using System.Linq;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Organization;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class OrganizationMapper
{
    public OrganizationNodeResponse ToOrganizationNodeResponse(OrganizationNode node)
    {
        if (node is null) return null;
        return new OrganizationNodeResponse
        {
            Id = node.Id.Value.ToString(),
            FullName = node.FullName,
            EmployeeCode = node.EmployeeCode,
            ManagerId = node.ManagerId?.Value.ToString(),
            ManagerName = node.ManagerName,
            DepartmentId = node.DepartmentId.Value.ToString(),
            DepartmentName = node.DepartmentName,
            PositionName = node.PositionName,
            GradeCode = node.GradeCode,
            DirectReports = node.DirectReports.Select(ToOrganizationNodeResponse).ToList()
        };
    }

    public ReportingRelationshipResponse ToReportingRelationshipResponse(ReportingRelationship relationship)
    {
        if (relationship is null) return null;
        return new ReportingRelationshipResponse
        {
            EmployeeId = relationship.EmployeeId.Value.ToString(),
            EmployeeName = relationship.EmployeeName,
            ManagerEmployeeId = relationship.ManagerEmployeeId?.Value.ToString(),
            ManagerName = relationship.ManagerName,
            Level = relationship.Level
        };
    }
}
