using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Recruitment;

public class CandidateDomainTests
{
    private static Candidate CreateActiveCandidate()
    {
        return new Candidate
        {
            Id = new CandidateId(Guid.NewGuid()),
            CandidateCode = "CAND-001",
            FullName = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "0901234567",
            Source = CandidateSourceCode.Website,
            CreatedBy = "user1",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "user1",
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void CreateCandidate_DefaultStatus_ShouldBeActive()
    {
        var candidate = CreateActiveCandidate();
        candidate.Status.Should().Be(CandidateStatusCode.Active);
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldSucceed()
    {
        var candidate = CreateActiveCandidate();
        var result = candidate.UpdateProfile("Jane Doe", "jane@example.com", "0987654321",
            new DateOnly(1990, 1, 1), "123 Main St", "https://resume.url", "Good candidate");
        result.Should().BeTrue();
        candidate.FullName.Should().Be("Jane Doe");
        candidate.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public void UpdateProfile_EmptyFullName_ShouldFail()
    {
        var candidate = CreateActiveCandidate();
        var result = candidate.UpdateProfile("", "jane@example.com", null, null, null, null, null);
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateProfile_EmptyEmail_ShouldFail()
    {
        var candidate = CreateActiveCandidate();
        var result = candidate.UpdateProfile("Jane Doe", "", null, null, null, null, null);
        result.Should().BeFalse();
    }

    [Fact]
    public void Blacklist_FromActive_ShouldSucceed()
    {
        var candidate = CreateActiveCandidate();
        var result = candidate.Blacklist("user1", DateTime.UtcNow);
        result.Should().BeTrue();
        candidate.Status.Should().Be(CandidateStatusCode.Blacklisted);
    }

    [Fact]
    public void Blacklist_FromBlacklisted_ShouldFail()
    {
        var candidate = CreateActiveCandidate();
        candidate.Blacklist("user1", DateTime.UtcNow);
        var result = candidate.Blacklist("user1", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void Archive_FromActive_ShouldSucceed()
    {
        var candidate = CreateActiveCandidate();
        var result = candidate.Archive("user1", DateTime.UtcNow);
        result.Should().BeTrue();
        candidate.Status.Should().Be(CandidateStatusCode.Archived);
    }

    [Fact]
    public void Archive_FromArchived_ShouldFail()
    {
        var candidate = CreateActiveCandidate();
        candidate.Archive("user1", DateTime.UtcNow);
        var result = candidate.Archive("user1", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void Reactivate_FromArchived_ShouldSucceed()
    {
        var candidate = CreateActiveCandidate();
        candidate.Archive("user1", DateTime.UtcNow);
        var result = candidate.Reactivate("user1", DateTime.UtcNow);
        result.Should().BeTrue();
        candidate.Status.Should().Be(CandidateStatusCode.Active);
    }

    [Fact]
    public void Reactivate_FromBlacklisted_ShouldFail()
    {
        var candidate = CreateActiveCandidate();
        candidate.Blacklist("user1", DateTime.UtcNow);
        var result = candidate.Reactivate("user1", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void Reactivate_FromActive_ShouldFail()
    {
        var candidate = CreateActiveCandidate();
        var result = candidate.Reactivate("user1", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void LinkEmployee_ShouldSetEmployeeId()
    {
        var candidate = CreateActiveCandidate();
        var employeeId = new EmployeeId(Guid.NewGuid());
        var result = candidate.LinkEmployee(employeeId, "user1", DateTime.UtcNow);
        result.Should().BeTrue();
        candidate.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public void LinkEmployee_WhenAlreadyLinked_ShouldFail()
    {
        var candidate = CreateActiveCandidate();
        var employeeId1 = new EmployeeId(Guid.NewGuid());
        var employeeId2 = new EmployeeId(Guid.NewGuid());
        candidate.LinkEmployee(employeeId1, "user1", DateTime.UtcNow);
        var result = candidate.LinkEmployee(employeeId2, "user1", DateTime.UtcNow);
        result.Should().BeFalse();
        candidate.EmployeeId.Should().Be(employeeId1);
    }

    [Fact]
    public void ChangeSource_ShouldUpdate()
    {
        var candidate = CreateActiveCandidate();
        candidate.ChangeSource(CandidateSourceCode.LinkedIn, "user1");
        candidate.Source.Should().Be(CandidateSourceCode.LinkedIn);
    }

    [Fact]
    public void Search_FilterByStatus_ShouldMatch()
    {
        var active = CreateActiveCandidate();
        active.Status.Should().Be(CandidateStatusCode.Active);

        var archived = CreateActiveCandidate();
        archived.Archive("user1", DateTime.UtcNow);
        archived.Status.Should().Be(CandidateStatusCode.Archived);

        var blacklisted = CreateActiveCandidate();
        blacklisted.Blacklist("user1", DateTime.UtcNow);
        blacklisted.Status.Should().Be(CandidateStatusCode.Blacklisted);
    }
}
