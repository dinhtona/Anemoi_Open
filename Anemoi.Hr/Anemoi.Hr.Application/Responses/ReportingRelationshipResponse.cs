namespace Anemoi.Hr.Application.Responses;

public sealed class ReportingRelationshipResponse
{
    public string EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string ManagerEmployeeId { get; set; }
    public string ManagerName { get; set; }
    public string Level { get; set; }
}
