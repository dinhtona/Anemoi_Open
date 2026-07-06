using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Separations.Events;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Separations;

public sealed class EmployeeSeparation : Entity<EmployeeSeparationId>
{
    public EmployeeId EmployeeId { get; set; }
    public string SeparationTypeCode { get; set; }
    public string StatusCode { get; set; }
    public DateOnly SeparationDate { get; set; }
    public DateOnly? LastWorkingDate { get; set; }
    public string? Reason { get; set; }
    public string? Details { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewComment { get; set; }
    public WorkflowInstanceId? WorkflowInstanceId { get; set; }

    public Employee Employee { get; set; }

    public static EmployeeSeparation Create(
        EmployeeSeparationId id,
        EmployeeId employeeId,
        string separationTypeCode,
        DateOnly separationDate,
        DateOnly? lastWorkingDate,
        string reason,
        string createdBy)
    {
        var separation = new EmployeeSeparation
        {
            Id = id,
            EmployeeId = employeeId,
            SeparationTypeCode = separationTypeCode,
            StatusCode = SeparationStatusCode.Draft,
            SeparationDate = separationDate,
            LastWorkingDate = lastWorkingDate,
            Reason = reason,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
        separation.AddEvent(new SeparationSubmittedDomainEvent(id, employeeId, createdBy));
        return separation;
    }

    public void Approve(string reviewer)
    {
        StatusCode = SeparationStatusCode.Approved;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewer;
        AddEvent(new SeparationApprovedDomainEvent(Id, EmployeeId, reviewer));
    }

    public void Reject(string reviewer, string comment)
    {
        StatusCode = SeparationStatusCode.Rejected;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewer;
        ReviewComment = comment;
        AddEvent(new SeparationRejectedDomainEvent(Id, EmployeeId, comment, reviewer));
    }
}
