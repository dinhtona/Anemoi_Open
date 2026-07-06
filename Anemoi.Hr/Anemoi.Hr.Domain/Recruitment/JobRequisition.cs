using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class JobRequisition : ValueObject
{
    public JobRequisitionId Id { get; set; }
    public string RequisitionCode { get; set; }
    public string Title { get; set; }
    public DepartmentId DepartmentId { get; set; }
    public PositionId PositionId { get; set; }
    public int Headcount { get; set; }
    public string EmploymentType { get; set; }
    public string RequestedBy { get; set; }
    public string ApprovedBy { get; set; }
    public DateOnly OpenDate { get; set; }
    public DateOnly TargetHireDate { get; set; }
    public string Description { get; set; }
    public string Status { get; private set; } = RequisitionStatusCode.Draft;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? SubmittedAt { get; private set; }
    public string SubmittedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string RejectedBy { get; private set; }
    public string RejectionReason { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string ClosedBy { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string CancelledBy { get; private set; }
    public string CancellationReason { get; private set; }

    // Navigation
    public Department Department { get; set; }
    public Position Position { get; set; }

    public bool Submit(string actor, DateTime now)
    {
        if (Status != RequisitionStatusCode.Draft)
            return false;

        Status = RequisitionStatusCode.Submitted;
        SubmittedBy = actor;
        SubmittedAt = now;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool Approve(string actor, DateTime now)
    {
        if (Status != RequisitionStatusCode.Submitted)
            return false;

        Status = RequisitionStatusCode.Approved;
        ApprovedBy = actor;
        ApprovedAt = now;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool Reject(string actor, DateTime now, string reason)
    {
        if (Status != RequisitionStatusCode.Submitted)
            return false;

        if (string.IsNullOrWhiteSpace(reason))
            return false;

        Status = RequisitionStatusCode.Rejected;
        RejectedBy = actor;
        RejectedAt = now;
        RejectionReason = reason;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool Close(string actor, DateTime now)
    {
        if (Status != RequisitionStatusCode.Approved)
            return false;

        Status = RequisitionStatusCode.Closed;
        ClosedBy = actor;
        ClosedAt = now;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool Cancel(string actor, DateTime now, string reason)
    {
        if (Status == RequisitionStatusCode.Closed ||
            Status == RequisitionStatusCode.Cancelled)
            return false;

        if (string.IsNullOrWhiteSpace(reason))
            return false;

        Status = RequisitionStatusCode.Cancelled;
        CancelledBy = actor;
        CancelledAt = now;
        CancellationReason = reason;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool CanCreatePosting => Status == RequisitionStatusCode.Approved;

    public bool CanModify => Status == RequisitionStatusCode.Draft || Status == RequisitionStatusCode.Rejected;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
