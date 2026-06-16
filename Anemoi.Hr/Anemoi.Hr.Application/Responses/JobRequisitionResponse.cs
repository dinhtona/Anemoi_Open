using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class JobRequisitionResponse
{
    public string Id { get; set; }
    public string RequisitionCode { get; set; }
    public string Title { get; set; }
    public string DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public string PositionId { get; set; }
    public string PositionName { get; set; }
    public int Headcount { get; set; }
    public string EmploymentType { get; set; }
    public string RequestedBy { get; set; }
    public string ApprovedBy { get; set; }
    public DateOnly OpenDate { get; set; }
    public DateOnly TargetHireDate { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string SubmittedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string RejectedBy { get; set; }
    public string RejectionReason { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string ClosedBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string CancelledBy { get; set; }
    public string CancellationReason { get; set; }
}
