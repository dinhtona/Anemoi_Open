using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Transfers.Events;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Transfers;

public sealed class EmployeeTransfer : Entity<EmployeeTransferId>
{
    public EmployeeId EmployeeId { get; set; }
    public DepartmentId SourceDepartmentId { get; set; }
    public DepartmentId TargetDepartmentId { get; set; }
    public PositionId SourcePositionId { get; set; }
    public PositionId TargetPositionId { get; set; }
    public string StatusCode { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string? Reason { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewComment { get; set; }
    public EmployeeId? SourceManagerId { get; set; }
    public EmployeeId? TargetManagerId { get; set; }
    public string SourceGradeCode { get; set; }
    public string TargetGradeCode { get; set; }
    public WorkflowInstanceId? WorkflowInstanceId { get; set; }

    public Employee Employee { get; set; }
    public Department SourceDepartment { get; set; }
    public Department TargetDepartment { get; set; }
    public Position SourcePosition { get; set; }
    public Position TargetPosition { get; set; }

    public static EmployeeTransfer Create(
        EmployeeTransferId id,
        EmployeeId employeeId,
        DepartmentId sourceDepartmentId,
        DepartmentId targetDepartmentId,
        PositionId sourcePositionId,
        PositionId targetPositionId,
        EmployeeId? sourceManagerId,
        EmployeeId? targetManagerId,
        string sourceGradeCode,
        string targetGradeCode,
        DateOnly effectiveDate,
        string reason,
        string createdBy)
    {
        var transfer = new EmployeeTransfer
        {
            Id = id,
            EmployeeId = employeeId,
            SourceDepartmentId = sourceDepartmentId,
            TargetDepartmentId = targetDepartmentId,
            SourcePositionId = sourcePositionId,
            TargetPositionId = targetPositionId,
            SourceManagerId = sourceManagerId,
            TargetManagerId = targetManagerId,
            SourceGradeCode = sourceGradeCode,
            TargetGradeCode = targetGradeCode,
            StatusCode = TransferStatusCode.Draft,
            EffectiveDate = effectiveDate,
            Reason = reason,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
        return transfer;
    }

    public void Submit()
    {
        StatusCode = TransferStatusCode.PendingApproval;
        AddEvent(new TransferSubmittedDomainEvent(Id, EmployeeId, CreatedBy));
    }

    public void Approve(string reviewer)
    {
        StatusCode = TransferStatusCode.Approved;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewer;
        AddEvent(new TransferApprovedDomainEvent(Id, EmployeeId, reviewer));
    }

    public void Reject(string reviewer, string comment)
    {
        StatusCode = TransferStatusCode.Rejected;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewer;
        ReviewComment = comment;
        AddEvent(new TransferRejectedDomainEvent(Id, EmployeeId, comment, reviewer));
    }
}
