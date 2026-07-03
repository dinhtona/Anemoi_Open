using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Events;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class RecruitmentRequest : Entity<RecruitmentRequestId>
{
    public string RequestNumber { get; set; }
    public DepartmentId DepartmentId { get; set; }
    public PositionId PositionId { get; set; }
    public int RequestedHeadcount { get; set; }
    public string Reason { get; set; }
    public string PriorityCode { get; set; }
    public string RequestedBy { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public string Status { get; private set; } = RecruitmentRequestStatusCode.Draft;
    public string ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string RejectedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string Comment { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Department Department { get; set; }
    public Position Position { get; set; }
    public ICollection<RecruitmentRequestHistory> Histories { get; set; } = new List<RecruitmentRequestHistory>();

    public static RecruitmentRequest Create(
        RecruitmentRequestId id,
        string requestNumber,
        DepartmentId departmentId,
        PositionId positionId,
        int requestedHeadcount,
        string reason,
        string priorityCode,
        string requestedBy,
        DateTime now)
    {
        return new RecruitmentRequest
        {
            Id = id,
            RequestNumber = requestNumber,
            DepartmentId = departmentId,
            PositionId = positionId,
            RequestedHeadcount = requestedHeadcount,
            Reason = reason,
            PriorityCode = priorityCode,
            RequestedBy = requestedBy,
            RequestedAt = now,
            Status = RecruitmentRequestStatusCode.Draft,
            ApprovedBy = string.Empty,
            RejectedBy = string.Empty,
            Comment = string.Empty,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public bool Submit(string actor, DateTime now)
    {
        if (Status != RecruitmentRequestStatusCode.Draft)
            return false;

        Status = RecruitmentRequestStatusCode.Submitted;
        RequestedBy = actor;
        RequestedAt = now;
        UpdatedAt = now;

        AddEvent(new RecruitmentRequestSubmittedDomainEvent(Id.Value, RequestNumber, actor));

        return true;
    }

    public bool Approve(string actor, DateTime now, string? comment = null)
    {
        if (Status != RecruitmentRequestStatusCode.Submitted)
            return false;

        Status = RecruitmentRequestStatusCode.Approved;
        ApprovedBy = actor;
        ApprovedAt = now;
        Comment = comment ?? string.Empty;
        UpdatedAt = now;

        AddEvent(new RecruitmentRequestApprovedDomainEvent(Id.Value, RequestNumber, actor));

        return true;
    }

    public bool Reject(string actor, DateTime now, string? comment = null)
    {
        if (Status != RecruitmentRequestStatusCode.Submitted)
            return false;

        Status = RecruitmentRequestStatusCode.Rejected;
        RejectedBy = actor;
        RejectedAt = now;
        Comment = comment ?? string.Empty;
        UpdatedAt = now;

        AddEvent(new RecruitmentRequestRejectedDomainEvent(Id.Value, RequestNumber, actor, comment ?? string.Empty));

        return true;
    }

    public bool Cancel(string actor, DateTime now)
    {
        if (Status == RecruitmentRequestStatusCode.Approved ||
            Status == RecruitmentRequestStatusCode.Rejected ||
            Status == RecruitmentRequestStatusCode.Cancelled)
            return false;

        Status = RecruitmentRequestStatusCode.Cancelled;
        UpdatedAt = now;

        return true;
    }

    public bool CanModify => Status == RecruitmentRequestStatusCode.Draft;
    public bool CanSubmit => Status == RecruitmentRequestStatusCode.Draft;
    public bool CanApprove => Status == RecruitmentRequestStatusCode.Submitted;
    public bool CanReject => Status == RecruitmentRequestStatusCode.Submitted;
    public bool CanCancel => Status == RecruitmentRequestStatusCode.Draft || Status == RecruitmentRequestStatusCode.Submitted;
}
