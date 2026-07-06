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

    public CandidateApplicationResponse ToResponse(CandidateApplication app)
    {
        if (app is null) return null;
        return new CandidateApplicationResponse
        {
            Id = app.Id.Value.ToString(),
            CandidateId = app.CandidateId.Value.ToString(),
            CandidateName = app.Candidate?.FullName,
            CandidateEmail = app.Candidate?.Email,
            JobPostingId = app.JobPostingId.Value.ToString(),
            PostingTitle = app.JobPosting?.PostingTitle,
            RequisitionCode = app.JobPosting?.JobRequisition?.RequisitionCode,
            AppliedAt = app.AppliedAt,
            CurrentStage = app.CurrentStage,
            CreatedAt = app.CreatedAt,
            UpdatedAt = app.UpdatedAt,
            StageHistories = app.StageHistories?.Select(ToResponse).ToList() ?? []
        };
    }

    public CandidateApplicationStageHistoryResponse ToResponse(CandidateApplicationStageHistory h)
    {
        if (h is null) return null;
        return new CandidateApplicationStageHistoryResponse
        {
            Id = h.Id.Value.ToString(),
            CandidateApplicationId = h.CandidateApplicationId.Value.ToString(),
            FromStage = h.FromStage,
            ToStage = h.ToStage,
            ChangedBy = h.ChangedBy,
            ChangedAt = h.ChangedAt,
            Note = h.Note
        };
    }

    public IReadOnlyCollection<CandidateApplicationResponse> ToApplicationResponses(
        IEnumerable<CandidateApplication> apps)
    {
        if (apps is null) return [];
        return apps.Select(ToResponse).ToList();
    }

    public IReadOnlyCollection<CandidateApplicationStageHistoryResponse> ToStageHistoryResponses(
        IEnumerable<CandidateApplicationStageHistory> history)
    {
        if (history is null) return [];
        return history.Select(ToResponse).ToList();
    }

    public InterviewScheduleResponse ToResponse(InterviewSchedule interview)
    {
        if (interview is null) return null;
        return new InterviewScheduleResponse
        {
            Id = interview.Id.Value.ToString(),
            CandidateApplicationId = interview.CandidateApplicationId.Value.ToString(),
            CandidateName = interview.CandidateApplication?.Candidate?.FullName,
            PostingTitle = interview.CandidateApplication?.JobPosting?.PostingTitle,
            CurrentStage = interview.CandidateApplication?.CurrentStage,
            InterviewType = interview.InterviewType,
            ScheduledAt = interview.ScheduledAt,
            DurationMinutes = interview.DurationMinutes,
            InterviewerEmployeeId = interview.InterviewerEmployeeId.Value.ToString(),
            InterviewerName = interview.Interviewer?.FullName,
            Notes = interview.Notes,
            Result = interview.Result,
            CreatedAt = interview.CreatedAt,
            CreatedBy = interview.CreatedBy,
            UpdatedAt = interview.UpdatedAt,
            UpdatedBy = interview.UpdatedBy,
            Feedbacks = interview.Feedbacks?.Select(ToResponse).ToList() ?? []
        };
    }

    public InterviewFeedbackResponse ToResponse(InterviewFeedback feedback)
    {
        if (feedback is null) return null;
        return new InterviewFeedbackResponse
        {
            Id = feedback.Id.Value.ToString(),
            InterviewScheduleId = feedback.InterviewScheduleId.Value.ToString(),
            InterviewerEmployeeId = feedback.InterviewerEmployeeId.Value.ToString(),
            InterviewerName = feedback.Interviewer?.FullName,
            Rating = feedback.Rating,
            Strengths = feedback.Strengths,
            Concerns = feedback.Concerns,
            Recommendation = feedback.Recommendation,
            CreatedAt = feedback.CreatedAt
        };
    }

    public IReadOnlyCollection<InterviewScheduleResponse> ToInterviewResponses(
        IEnumerable<InterviewSchedule> interviews)
    {
        if (interviews is null) return [];
        return interviews.Select(ToResponse).ToList();
    }

    public HiringDecisionResponse ToResponse(HiringDecision decision)
    {
        if (decision is null) return null;
        return new HiringDecisionResponse
        {
            Id = decision.Id.Value.ToString(),
            CandidateApplicationId = decision.CandidateApplicationId.Value.ToString(),
            CandidateName = decision.CandidateApplication?.Candidate?.FullName,
            PostingTitle = decision.CandidateApplication?.JobPosting?.PostingTitle,
            CurrentStage = decision.CandidateApplication?.CurrentStage,
            Decision = decision.Decision,
            DecidedBy = decision.DecidedBy,
            DecidedAt = decision.DecidedAt,
            Notes = decision.Notes,
            CreatedAt = decision.CreatedAt
        };
    }

    public IReadOnlyCollection<HiringDecisionResponse> ToHiringDecisionResponses(
        IEnumerable<HiringDecision> decisions)
    {
        if (decisions is null) return [];
        return decisions.Select(ToResponse).ToList();
    }

    public RecruitmentRequestResponse ToResponse(RecruitmentRequest req)
    {
        if (req is null) return null;
        return new RecruitmentRequestResponse(
            Id: req.Id.Value.ToString(),
            RequestNumber: req.RequestNumber,
            DepartmentId: req.DepartmentId.Value.ToString(),
            DepartmentName: req.Department?.Name,
            PositionId: req.PositionId.Value.ToString(),
            PositionName: req.Position?.Name,
            RequestedHeadcount: req.RequestedHeadcount,
            Reason: req.Reason,
            PriorityCode: req.PriorityCode,
            RequestedBy: req.RequestedBy,
            RequestedAt: req.RequestedAt,
            Status: req.Status,
            ApprovedBy: req.ApprovedBy,
            ApprovedAt: req.ApprovedAt,
            RejectedBy: req.RejectedBy,
            RejectedAt: req.RejectedAt,
            Comment: req.Comment,
            CreatedAt: req.CreatedAt,
            UpdatedAt: req.UpdatedAt
        );
    }

    public IReadOnlyCollection<RecruitmentRequestResponse> ToResponses(IEnumerable<RecruitmentRequest> reqs)
    {
        if (reqs is null) return [];
        return reqs.Select(ToResponse).ToList();
    }

    public RecruitmentRequestHistoryResponse ToResponse(RecruitmentRequestHistory h)
    {
        if (h is null) return null;
        return new RecruitmentRequestHistoryResponse(
            Id: h.Id.Value.ToString(),
            RecruitmentRequestId: h.RecruitmentRequestId.Value.ToString(),
            ActionCode: h.ActionCode,
            OldStatus: h.OldStatus,
            NewStatus: h.NewStatus,
            Comment: h.Comment,
            PerformedBy: h.PerformedBy,
            PerformedAt: h.PerformedAt
        );
    }

    public IReadOnlyCollection<RecruitmentRequestHistoryResponse> ToHistoryResponses(
        IEnumerable<RecruitmentRequestHistory> history)
    {
        if (history is null) return [];
        return history.Select(ToResponse).ToList();
    }

    public RecruitmentOpeningResponse ToResponse(RecruitmentOpening opening)
    {
        if (opening is null) return null;
        return new RecruitmentOpeningResponse(
            Id: opening.Id.Value.ToString(),
            RecruitmentRequestId: opening.RecruitmentRequestId.Value.ToString(),
            Code: opening.Code,
            PlannedHeadcount: opening.PlannedHeadcount,
            FilledHeadcount: opening.FilledHeadcount,
            RemainingHeadcount: opening.RemainingHeadcount,
            Status: opening.Status,
            OpenedAt: opening.OpenedAt
        );
    }

    public IReadOnlyCollection<RecruitmentOpeningResponse> ToOpeningResponses(
        IEnumerable<RecruitmentOpening> openings)
    {
        if (openings is null) return [];
        return openings.Select(ToResponse).ToList();
    }
}
