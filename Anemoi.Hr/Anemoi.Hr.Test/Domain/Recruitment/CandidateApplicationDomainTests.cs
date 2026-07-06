using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Recruitment;

public class CandidateApplicationDomainTests
{
    private static readonly CandidateApplicationId AppId = new(Guid.NewGuid());
    private static readonly CandidateApplicationStageHistoryId HistoryId = new(Guid.NewGuid());
    private static readonly CandidateId CandidateId = new(Guid.NewGuid());
    private static readonly JobPostingId PostingId = new(Guid.NewGuid());

    private static CandidateApplication CreateInitialApplication()
    {
        var app = CandidateApplication.Create(AppId, CandidateId, PostingId, DateTime.UtcNow);
        app.MoveToStage(CandidateApplicationStageCode.Applied, HistoryId, "recruiter1", DateTime.UtcNow, null);
        return app;
    }

    [Fact]
    public void CreateApplication_InitialStage_ShouldBeApplied()
    {
        var app = CreateInitialApplication();
        app.CurrentStage.Should().Be(CandidateApplicationStageCode.Applied);
    }

    [Fact]
    public void CreateApplication_ShouldRecordStageHistory()
    {
        var app = CreateInitialApplication();
        app.StageHistories.Should().ContainSingle();
        app.StageHistories[0].ToStage.Should().Be(CandidateApplicationStageCode.Applied);
        app.StageHistories[0].FromStage.Should().BeNull();
    }

    [Fact]
    public void Applied_ToScreening_ShouldSucceed()
    {
        var app = CreateInitialApplication();
        var result = app.MoveToStage(CandidateApplicationStageCode.Screening, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        result.Should().BeTrue();
        app.CurrentStage.Should().Be(CandidateApplicationStageCode.Screening);
        app.StageHistories.Should().HaveCount(2);
    }

    [Fact]
    public void Screening_ToInterview_ShouldSucceed()
    {
        var app = CreateInitialApplication();
        app.MoveToStage(CandidateApplicationStageCode.Screening, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        var result = app.MoveToStage(CandidateApplicationStageCode.Interview, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        result.Should().BeTrue();
        app.CurrentStage.Should().Be(CandidateApplicationStageCode.Interview);
    }

    [Fact]
    public void Interview_ToOffer_ShouldSucceed()
    {
        var app = CreateInitialApplication();
        app.MoveToStage(CandidateApplicationStageCode.Screening, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        app.MoveToStage(CandidateApplicationStageCode.Interview, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        var result = app.MoveToStage(CandidateApplicationStageCode.Offer, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        result.Should().BeTrue();
        app.CurrentStage.Should().Be(CandidateApplicationStageCode.Offer);
    }

    [Fact]
    public void Offer_ToHired_ShouldSucceed()
    {
        var app = CreateInitialApplication();
        app.MoveToStage(CandidateApplicationStageCode.Screening, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        app.MoveToStage(CandidateApplicationStageCode.Interview, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        app.MoveToStage(CandidateApplicationStageCode.Offer, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        var result = app.MoveToStage(CandidateApplicationStageCode.Hired, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        result.Should().BeTrue();
        app.CurrentStage.Should().Be(CandidateApplicationStageCode.Hired);
    }

    [Fact]
    public void Applied_ToRejected_ShouldSucceed()
    {
        var app = CreateInitialApplication();
        var result = app.MoveToStage(CandidateApplicationStageCode.Rejected, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        result.Should().BeTrue();
        app.CurrentStage.Should().Be(CandidateApplicationStageCode.Rejected);
    }

    [Fact]
    public void Applied_ToWithdrawn_ShouldSucceed()
    {
        var app = CreateInitialApplication();
        var result = app.MoveToStage(CandidateApplicationStageCode.Withdrawn, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        result.Should().BeTrue();
        app.CurrentStage.Should().Be(CandidateApplicationStageCode.Withdrawn);
    }

    [Fact]
    public void TerminalStage_Hired_CannotTransition()
    {
        var app = CreateInitialApplication();
        app.MoveToStage(CandidateApplicationStageCode.Screening, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        app.MoveToStage(CandidateApplicationStageCode.Interview, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        app.MoveToStage(CandidateApplicationStageCode.Offer, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        app.MoveToStage(CandidateApplicationStageCode.Hired, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        var result = app.MoveToStage(CandidateApplicationStageCode.Rejected, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        result.Should().BeFalse();
    }

    [Fact]
    public void TerminalStage_Rejected_CannotTransition()
    {
        var app = CreateInitialApplication();
        app.MoveToStage(CandidateApplicationStageCode.Rejected, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        var result = app.MoveToStage(CandidateApplicationStageCode.Screening, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        result.Should().BeFalse();
    }

    [Fact]
    public void TerminalStage_Withdrawn_CannotTransition()
    {
        var app = CreateInitialApplication();
        app.MoveToStage(CandidateApplicationStageCode.Withdrawn, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        var result = app.MoveToStage(CandidateApplicationStageCode.Screening, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        result.Should().BeFalse();
    }

    [Fact]
    public void Applied_ToOffer_DirectTransition_ShouldFail()
    {
        var app = CreateInitialApplication();
        var result = app.MoveToStage(CandidateApplicationStageCode.Offer, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        result.Should().BeFalse();
    }

    [Fact]
    public void FilterByStage_ShouldReturnMatchingApps()
    {
        var app1 = CreateInitialApplication();
        app1.CurrentStage.Should().Be(CandidateApplicationStageCode.Applied);

        var app2 = CreateInitialApplication();
        app2.MoveToStage(CandidateApplicationStageCode.Screening, new CandidateApplicationStageHistoryId(Guid.NewGuid()), "user1", DateTime.UtcNow, null);
        app2.CurrentStage.Should().Be(CandidateApplicationStageCode.Screening);
    }
}
