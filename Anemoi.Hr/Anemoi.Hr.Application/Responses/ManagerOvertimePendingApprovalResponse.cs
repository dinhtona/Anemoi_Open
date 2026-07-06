namespace Anemoi.Hr.Application.Responses;

public sealed class ManagerOvertimePendingApprovalResponse
{
    public string RequestId { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string DepartmentName { get; set; }
    public DateOnly OvertimeDate { get; set; }
    public decimal Hours { get; set; }
    public string Reason { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; }
    public string WorkflowInstanceId { get; set; }
    public string CurrentStepName { get; set; }
    public string WorkflowStatus { get; set; }
    public string CurrentApproverName { get; set; }
}
