using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Recruitment;

public class JobPostingDomainTests
{
    private static JobRequisition CreateApprovedRequisition()
    {
        var req = new JobRequisition
        {
            Id = new JobRequisitionId(Guid.NewGuid()),
            RequisitionCode = "REQ-001",
            Title = "Test Requisition",
            Headcount = 1,
            EmploymentType = "FullTime",
            RequestedBy = "user1",
            OpenDate = new DateOnly(2026, 1, 1),
            TargetHireDate = new DateOnly(2026, 3, 1),
            CreatedBy = "user1",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "user1",
            UpdatedAt = DateTime.UtcNow
        };
        req.Submit("user1", DateTime.UtcNow);
        req.Approve("user2", DateTime.UtcNow);
        return req;
    }

    private static JobPosting CreateDraftPosting(JobRequisition requisition = null)
    {
        requisition ??= CreateApprovedRequisition();
        return new JobPosting
        {
            Id = new JobPostingId(Guid.NewGuid()),
            JobRequisitionId = requisition.Id,
            PostingTitle = "Software Engineer",
            PostingDescription = "Hiring a software engineer",
            PublishDate = new DateOnly(2026, 2, 1),
            ExpiryDate = new DateOnly(2026, 4, 1),
            CreatedBy = "user1",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "user1",
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Publish_FromDraft_ShouldSucceed()
    {
        var posting = CreateDraftPosting();
        var result = posting.Publish("user1", DateTime.UtcNow);
        result.Should().BeTrue();
        posting.Status.Should().Be(JobPostingStatusCode.Published);
        posting.PublishedBy.Should().Be("user1");
    }

    [Fact]
    public void Publish_FromPublished_ShouldFail()
    {
        var posting = CreateDraftPosting();
        posting.Publish("user1", DateTime.UtcNow);
        var result = posting.Publish("user2", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void Publish_InvalidDateRange_ShouldFail()
    {
        var posting = CreateDraftPosting();
        posting.PublishDate = new DateOnly(2026, 5, 1);
        posting.ExpiryDate = new DateOnly(2026, 4, 1);
        var result = posting.Publish("user1", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldSucceed()
    {
        var posting = CreateDraftPosting();
        var result = posting.UpdateDetails("Senior Engineer", "Updated", new DateOnly(2026, 3, 1), new DateOnly(2026, 6, 1));
        result.Should().BeTrue();
        posting.PostingTitle.Should().Be("Senior Engineer");
    }

    [Fact]
    public void UpdateDetails_InvalidDateRange_ShouldFail()
    {
        var posting = CreateDraftPosting();
        var result = posting.UpdateDetails("Title", "Desc", new DateOnly(2026, 5, 1), new DateOnly(2026, 4, 1));
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateDetails_WhenClosed_ShouldFail()
    {
        var posting = CreateDraftPosting();
        posting.Close("user1", DateTime.UtcNow);
        var result = posting.UpdateDetails("Title", "Desc", new DateOnly(2026, 3, 1), new DateOnly(2026, 6, 1));
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateDetails_WhenExpired_ShouldFail()
    {
        var posting = CreateDraftPosting();
        posting.Publish("user1", DateTime.UtcNow);
        posting.Expire("user1", DateTime.UtcNow);
        var result = posting.UpdateDetails("Title", "Desc", new DateOnly(2026, 3, 1), new DateOnly(2026, 6, 1));
        result.Should().BeFalse();
    }

    [Fact]
    public void Expire_FromPublished_ShouldSucceed()
    {
        var posting = CreateDraftPosting();
        posting.Publish("user1", DateTime.UtcNow);
        var result = posting.Expire("user1", DateTime.UtcNow);
        result.Should().BeTrue();
        posting.Status.Should().Be(JobPostingStatusCode.Expired);
    }

    [Fact]
    public void Expire_FromDraft_ShouldFail()
    {
        var posting = CreateDraftPosting();
        var result = posting.Expire("user1", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void Close_FromDraft_ShouldSucceed()
    {
        var posting = CreateDraftPosting();
        var result = posting.Close("user1", DateTime.UtcNow);
        result.Should().BeTrue();
        posting.Status.Should().Be(JobPostingStatusCode.Closed);
    }

    [Fact]
    public void Close_FromPublished_ShouldSucceed()
    {
        var posting = CreateDraftPosting();
        posting.Publish("user1", DateTime.UtcNow);
        var result = posting.Close("user1", DateTime.UtcNow);
        result.Should().BeTrue();
        posting.Status.Should().Be(JobPostingStatusCode.Closed);
    }

    [Fact]
    public void Close_FromClosed_ShouldFail()
    {
        var posting = CreateDraftPosting();
        posting.Close("user1", DateTime.UtcNow);
        var result = posting.Close("user2", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void ApprovedRequisition_CanCreatePosting_ShouldBeTrue()
    {
        var req = CreateApprovedRequisition();
        req.CanCreatePosting.Should().BeTrue();
    }

    [Fact]
    public void DraftRequisition_CanCreatePosting_ShouldBeFalse()
    {
        var req = new JobRequisition { Id = new JobRequisitionId(Guid.NewGuid()) };
        req.CanCreatePosting.Should().BeFalse();
    }

    [Fact]
    public void SubmittedRequisition_CanCreatePosting_ShouldBeFalse()
    {
        var req = new JobRequisition { Id = new JobRequisitionId(Guid.NewGuid()) };
        req.Submit("user1", DateTime.UtcNow);
        req.CanCreatePosting.Should().BeFalse();
    }

    [Fact]
    public void ClosedRequisition_CanCreatePosting_ShouldBeFalse()
    {
        var req = CreateApprovedRequisition();
        req.Close("user1", DateTime.UtcNow);
        req.CanCreatePosting.Should().BeFalse();
    }

    [Fact]
    public void CancelledRequisition_CanCreatePosting_ShouldBeFalse()
    {
        var req = CreateApprovedRequisition();
        req.Cancel("user1", DateTime.UtcNow, "No longer needed");
        req.CanCreatePosting.Should().BeFalse();
    }
}
