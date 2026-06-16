using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anemoi.Hr.Application.Mappings;

public sealed class RecruitmentMapper
{
    public JobRequisitionResponse ToResponse(JobRequisition req)
    {
        if (req is null) return null;
        return new JobRequisitionResponse
        {
            Id = req.Id.Value.ToString(),
            RequisitionCode = req.RequisitionCode,
            Title = req.Title,
            DepartmentId = req.DepartmentId.Value.ToString(),
            DepartmentName = req.Department?.Name,
            PositionId = req.PositionId.Value.ToString(),
            PositionName = req.Position?.Name,
            Headcount = req.Headcount,
            EmploymentType = req.EmploymentType,
            RequestedBy = req.RequestedBy,
            ApprovedBy = req.ApprovedBy,
            OpenDate = req.OpenDate,
            TargetHireDate = req.TargetHireDate,
            Description = req.Description,
            Status = req.Status,
            CreatedAt = req.CreatedAt,
            CreatedBy = req.CreatedBy,
            UpdatedAt = req.UpdatedAt,
            UpdatedBy = req.UpdatedBy,
            SubmittedAt = req.SubmittedAt,
            SubmittedBy = req.SubmittedBy,
            ApprovedAt = req.ApprovedAt,
            RejectedAt = req.RejectedAt,
            RejectedBy = req.RejectedBy,
            RejectionReason = req.RejectionReason,
            ClosedAt = req.ClosedAt,
            ClosedBy = req.ClosedBy,
            CancelledAt = req.CancelledAt,
            CancelledBy = req.CancelledBy,
            CancellationReason = req.CancellationReason
        };
    }

    public IReadOnlyCollection<JobRequisitionResponse> ToResponses(IEnumerable<JobRequisition> reqs)
    {
        if (reqs is null) return [];
        return reqs.Select(ToResponse).ToList();
    }
}
