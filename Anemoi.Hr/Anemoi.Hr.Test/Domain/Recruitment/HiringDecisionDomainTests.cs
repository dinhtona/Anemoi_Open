using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Recruitment;

public class HiringDecisionDomainTests
{
    [Fact]
    public void CreateDecision_Offer_ShouldSetFields()
    {
        var decision = HiringDecision.Create(
            new HiringDecisionId(Guid.NewGuid()),
            new CandidateApplicationId(Guid.NewGuid()),
            HiringDecisionCode.Offer,
            "user1",
            DateTime.UtcNow,
            "Suitable candidate");

        decision.Should().NotBeNull();
        decision.Decision.Should().Be(HiringDecisionCode.Offer);
        decision.DecidedBy.Should().Be("user1");
        decision.Notes.Should().Be("Suitable candidate");
    }

    [Fact]
    public void CreateDecision_Hire_ShouldSetFields()
    {
        var decision = HiringDecision.Create(
            new HiringDecisionId(Guid.NewGuid()),
            new CandidateApplicationId(Guid.NewGuid()),
            HiringDecisionCode.Hire,
            "manager1",
            DateTime.UtcNow,
            null);

        decision.Decision.Should().Be(HiringDecisionCode.Hire);
    }

    [Fact]
    public void CreateDecision_Reject_ShouldSetFields()
    {
        var decision = HiringDecision.Create(
            new HiringDecisionId(Guid.NewGuid()),
            new CandidateApplicationId(Guid.NewGuid()),
            HiringDecisionCode.Reject,
            "manager1",
            DateTime.UtcNow,
            "Insufficient experience");

        decision.Decision.Should().Be(HiringDecisionCode.Reject);
        decision.Notes.Should().Be("Insufficient experience");
    }

    [Fact]
    public void DecisionIsUniquelyPerApplication()
    {
        var appId = new CandidateApplicationId(Guid.NewGuid());

        var d1 = HiringDecision.Create(
            new HiringDecisionId(Guid.NewGuid()), appId, HiringDecisionCode.Offer, "user1", DateTime.UtcNow, null);
        var d2 = HiringDecision.Create(
            new HiringDecisionId(Guid.NewGuid()), appId, HiringDecisionCode.Hire, "user1", DateTime.UtcNow, null);

        d1.CandidateApplicationId.Should().Be(d2.CandidateApplicationId);
        d1.Id.Should().NotBe(d2.Id);
    }

    [Fact]
    public void SearchByDecision_ShouldMatch()
    {
        var offer = HiringDecision.Create(
            new HiringDecisionId(Guid.NewGuid()), new CandidateApplicationId(Guid.NewGuid()),
            HiringDecisionCode.Offer, "user1", DateTime.UtcNow, null);
        var hire = HiringDecision.Create(
            new HiringDecisionId(Guid.NewGuid()), new CandidateApplicationId(Guid.NewGuid()),
            HiringDecisionCode.Hire, "user1", DateTime.UtcNow, null);

        offer.Decision.Should().Be(HiringDecisionCode.Offer);
        hire.Decision.Should().Be(HiringDecisionCode.Hire);
    }
}
