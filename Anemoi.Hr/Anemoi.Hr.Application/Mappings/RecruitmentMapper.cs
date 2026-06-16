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

    public JobPostingResponse ToResponse(JobPosting posting)
    {
        if (posting is null) return null;
        return new JobPostingResponse
        {
            Id = posting.Id.Value.ToString(),
            JobRequisitionId = posting.JobRequisitionId.Value.ToString(),
            RequisitionCode = posting.JobRequisition?.RequisitionCode,
            RequisitionTitle = posting.JobRequisition?.Title,
            PostingTitle = posting.PostingTitle,
            PostingDescription = posting.PostingDescription,
            PublishDate = posting.PublishDate.ToString("yyyy-MM-dd"),
            ExpiryDate = posting.ExpiryDate.ToString("yyyy-MM-dd"),
            Status = posting.Status,
            CreatedAt = posting.CreatedAt,
            CreatedBy = posting.CreatedBy,
            UpdatedAt = posting.UpdatedAt,
            UpdatedBy = posting.UpdatedBy,
            PublishedAt = posting.PublishedAt,
            PublishedBy = posting.PublishedBy,
            ExpiredAt = posting.ExpiredAt,
            ExpiredBy = posting.ExpiredBy,
            ClosedAt = posting.ClosedAt,
            ClosedBy = posting.ClosedBy
        };
    }

    public IReadOnlyCollection<JobPostingResponse> ToPostingResponses(IEnumerable<JobPosting> postings)
    {
        if (postings is null) return [];
        return postings.Select(ToResponse).ToList();
    }

    public CandidateResponse ToResponse(Candidate candidate)
    {
        if (candidate is null) return null;
        return new CandidateResponse
        {
            Id = candidate.Id.Value.ToString(),
            CandidateCode = candidate.CandidateCode,
            FullName = candidate.FullName,
            Email = candidate.Email,
            PhoneNumber = candidate.PhoneNumber,
            DateOfBirth = candidate.DateOfBirth?.ToString("yyyy-MM-dd"),
            Address = candidate.Address,
            ResumeUrl = candidate.ResumeUrl,
            Source = candidate.Source,
            Status = candidate.Status,
            Notes = candidate.Notes,
            EmployeeId = candidate.EmployeeId?.Value.ToString(),
            ConvertedAt = candidate.ConvertedAt,
            ConvertedBy = candidate.ConvertedBy,
            CreatedAt = candidate.CreatedAt,
            CreatedBy = candidate.CreatedBy,
            UpdatedAt = candidate.UpdatedAt,
            UpdatedBy = candidate.UpdatedBy
        };
    }

    public IReadOnlyCollection<CandidateResponse> ToCandidateResponses(IEnumerable<Candidate> candidates)
    {
        if (candidates is null) return [];
        return candidates.Select(ToResponse).ToList();
    }
}
