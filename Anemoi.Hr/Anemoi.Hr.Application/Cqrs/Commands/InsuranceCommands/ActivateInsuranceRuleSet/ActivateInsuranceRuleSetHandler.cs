using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.ActivateInsuranceRuleSet;

public sealed class ActivateInsuranceRuleSetHandler(
    ISqlRepository<InsuranceRuleSet> ruleSetRepository,
    ISqlRepository<InsuranceContributionRule> contributionRuleRepository,
    ISqlRepository<InsuranceAuditLog> auditLogRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ActivateInsuranceRuleSetCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        ActivateInsuranceRuleSetCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var ruleSetGuid))
        {
            return HrErrorResponses.Create("HR_INSURANCE_RULE_SET_NOT_FOUND");
        }

        var ruleSetId = new InsuranceRuleSetId(ruleSetGuid);
        var ruleSet = await ruleSetRepository.GetFirstByConditionAsync(
            x => x.Id == ruleSetId,
            token: cancellationToken);

        if (ruleSet is null)
        {
            return HrErrorResponses.Create("HR_INSURANCE_RULE_SET_NOT_FOUND");
        }

        if (ruleSet.Status != InsuranceRuleSetStatuses.Draft)
        {
            return HrErrorResponses.Create("HR_INSURANCE_RULE_SET_NOT_DRAFT");
        }

        // Check if there are contribution rules defined
        var hasRules = await contributionRuleRepository.ExistByConditionAsync(
            x => x.RuleSetId == ruleSetId,
            token: cancellationToken);

        if (!hasRules)
        {
            return HrErrorResponses.Create("HR_INSURANCE_RULE_SET_HAS_NO_RULES");
        }

        // Check for overlaps with other ACTIVE sets for same CountryCode + InsuranceType
        var activeSets = await ruleSetRepository.GetManyByConditionAsync(
            x => x.CountryCode == ruleSet.CountryCode && x.InsuranceType == ruleSet.InsuranceType && x.Id != ruleSetId && x.Status == InsuranceRuleSetStatuses.Active,
            token: cancellationToken);

        var overlaps = activeSets.Any(x =>
            ruleSet.EffectiveFrom <= (x.EffectiveTo ?? DateOnly.MaxValue) &&
            (ruleSet.EffectiveTo ?? DateOnly.MaxValue) >= x.EffectiveFrom);

        if (overlaps)
        {
            return HrErrorResponses.Create("HR_INSURANCE_RULE_SET_OVERLAP_WITH_ACTIVE");
        }

        ruleSet.Status = InsuranceRuleSetStatuses.Active;
        ruleSet.UpdatedAt = DateTime.UtcNow;
        ruleSet.UpdatedBy = request.ActivatedBy ?? "system";

        var auditLog = new InsuranceAuditLog
        {
            Id = new InsuranceAuditLogId(IdGenerator.NextGuid()),
            RuleSetId = ruleSetId,
            Action = "Activate",
            Description = $"Insurance rule set '{ruleSet.Name}' activated",
            PerformedBy = request.ActivatedBy ?? "system",
            PerformedAt = DateTime.UtcNow
        };

        await auditLogRepository.CreateOneAsync(auditLog, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new SuccessResponse();
    }
}
