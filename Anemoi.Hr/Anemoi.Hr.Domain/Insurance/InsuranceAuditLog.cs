using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Domain.Insurance;

public sealed class InsuranceAuditLog : Entity<InsuranceAuditLogId>
{
    public InsuranceRuleSetId? RuleSetId { get; set; }
    public string Action { get; set; }
    public string Description { get; set; }
    public string PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }

    public InsuranceRuleSet RuleSet { get; set; }
}
