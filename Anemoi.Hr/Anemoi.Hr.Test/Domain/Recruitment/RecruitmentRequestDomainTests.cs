using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using System;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Recruitment;

public sealed class RecruitmentRequestDomainTests
{
    private static RecruitmentRequest CreateDraftRequest()
    {
        return new RecruitmentRequest
        {
            Id = new RecruitmentRequestId(Guid.NewGuid()),
            RequestNumber = "RR-202606-0001",
            DepartmentId = new DepartmentId(Guid.NewGuid()),
            PositionId = new PositionId(Guid.NewGuid()),
            RequestedHeadcount = 2,
            Reason = "New position needed",
            PriorityCode = RecruitmentRequestPriorityCode.High,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Submit_ShouldTransitionFromDraftToSubmitted()
    {
        var request = CreateDraftRequest();
        var result = request.Submit("user-1", DateTime.UtcNow);

        result.Should().BeTrue();
        request.Status.Should().Be(RecruitmentRequestStatusCode.Submitted);
    }

    [Fact]
    public void Submit_ShouldFail_WhenNotDraft()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);

        var result = request.Submit("user-2", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void Approve_ShouldTransitionFromSubmittedToApproved()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);

        var result = request.Approve("approver", DateTime.UtcNow, "Approved");

        result.Should().BeTrue();
        request.Status.Should().Be(RecruitmentRequestStatusCode.Approved);
        request.ApprovedBy.Should().Be("approver");
        request.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_ShouldFail_WhenNotSubmitted()
    {
        var request = CreateDraftRequest();
        var result = request.Approve("approver", DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void Reject_ShouldTransitionFromSubmittedToRejected()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);

        var result = request.Reject("approver", DateTime.UtcNow, "Not needed");

        result.Should().BeTrue();
        request.Status.Should().Be(RecruitmentRequestStatusCode.Rejected);
    }

    [Fact]
    public void Reject_ShouldFail_WhenNotSubmitted()
    {
        var request = CreateDraftRequest();
        var result = request.Reject("approver", DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ShouldTransitionFromDraftToCancelled()
    {
        var request = CreateDraftRequest();
        var result = request.Cancel("user-1", DateTime.UtcNow);

        result.Should().BeTrue();
        request.Status.Should().Be(RecruitmentRequestStatusCode.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldFail_WhenApproved()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.Approve("approver", DateTime.UtcNow);

        var result = request.Cancel("user-1", DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ShouldFail_WhenRejected()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.Reject("approver", DateTime.UtcNow);

        var result = request.Cancel("user-1", DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanModify_ShouldBeTrue_WhenDraft()
    {
        var request = CreateDraftRequest();
        request.CanModify.Should().BeTrue();
    }

    [Fact]
    public void CanModify_ShouldBeFalse_WhenSubmitted()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.CanModify.Should().BeFalse();
    }

    [Fact]
    public void CanSubmit_ShouldBeTrue_WhenDraft()
    {
        var request = CreateDraftRequest();
        request.CanSubmit.Should().BeTrue();
    }

    [Fact]
    public void CanSubmit_ShouldBeFalse_WhenSubmitted()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.CanSubmit.Should().BeFalse();
    }

    [Fact]
    public void CanApprove_ShouldBeTrue_WhenSubmitted()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.CanApprove.Should().BeTrue();
    }

    [Fact]
    public void CanApprove_ShouldBeFalse_WhenDraft()
    {
        var request = CreateDraftRequest();
        request.CanApprove.Should().BeFalse();
    }

    [Fact]
    public void CanCancel_ShouldBeTrue_WhenDraft()
    {
        var request = CreateDraftRequest();
        request.CanCancel.Should().BeTrue();
    }

    [Fact]
    public void CanCancel_ShouldBeTrue_WhenSubmitted()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.CanCancel.Should().BeTrue();
    }

    [Fact]
    public void CanCancel_ShouldBeFalse_WhenApproved()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.Approve("approver", DateTime.UtcNow);
        request.CanCancel.Should().BeFalse();
    }

    [Fact]
    public void RecruitingOpening_MarkFilled_ShouldIncrementFilledHeadcount()
    {
        var opening = new RecruitmentOpening
        {
            Id = new RecruitmentOpeningId(Guid.NewGuid()),
            RecruitmentRequestId = new RecruitmentRequestId(Guid.NewGuid()),
            Code = "OPEN-001",
            PlannedHeadcount = 5,
            FilledHeadcount = 2,
            Status = "Open",
            OpenedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        opening.MarkFilled(1);

        opening.FilledHeadcount.Should().Be(3);
        opening.RemainingHeadcount.Should().Be(2);
    }

    [Fact]
    public void RecruitingOpening_MarkFilled_ShouldCloseWhenFull()
    {
        var opening = new RecruitmentOpening
        {
            Id = new RecruitmentOpeningId(Guid.NewGuid()),
            RecruitmentRequestId = new RecruitmentRequestId(Guid.NewGuid()),
            Code = "OPEN-001",
            PlannedHeadcount = 3,
            FilledHeadcount = 2,
            Status = "Open",
            OpenedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        opening.MarkFilled(1);

        opening.FilledHeadcount.Should().Be(3);
        opening.RemainingHeadcount.Should().Be(0);
        opening.Status.Should().Be("Filled");
        opening.ClosedAt.Should().NotBeNull();
    }
}
