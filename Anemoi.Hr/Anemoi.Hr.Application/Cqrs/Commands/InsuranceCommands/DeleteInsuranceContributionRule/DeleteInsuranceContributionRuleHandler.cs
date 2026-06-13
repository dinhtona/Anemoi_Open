using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.DeleteInsuranceContributionRule;

public sealed class DeleteInsuranceContributionRuleHandler(
    ISqlRepository<InsuranceRuleSet> ruleSetRepository,
    ISqlRepository<InsuranceContributionRule> contributionRuleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteInsuranceContributionRuleCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeleteInsuranceContributionRuleCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var ruleGuid))
        {
            return HrErrorResponses.Create("HR_INSURANCE_CONTRIBUTION_RULE_NOT_FOUND");
        }

        var ruleId = new InsuranceContributionRuleId(ruleGuid);
        var rule = await contributionRuleRepository.GetFirstByConditionAsync(
            x => x.Id == ruleId,
            token: cancellationToken);

        if (rule is null)
        {
            return HrErrorResponses.Create("HR_INSURANCE_CONTRIBUTION_RULE_NOT_FOUND");
        }

        var ruleSet = await ruleSetRepository.GetFirstByConditionAsync(
            x => x.Id == rule.RuleSetId,
            token: cancellationToken);

        if (ruleSet is null || ruleSet.Status != InsuranceRuleSetStatuses.Draft)
        {
            return HrErrorResponses.Create("HR_INSURANCE_RULE_SET_NOT_DRAFT");
        }

        await contributionRuleRepository.RemoveOneAsync(rule, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new SuccessResponse();
    }
}
