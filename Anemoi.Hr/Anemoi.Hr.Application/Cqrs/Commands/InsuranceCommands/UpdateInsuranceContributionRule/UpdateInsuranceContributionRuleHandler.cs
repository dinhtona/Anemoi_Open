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

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.UpdateInsuranceContributionRule;

public sealed class UpdateInsuranceContributionRuleHandler(
    ISqlRepository<InsuranceRuleSet> ruleSetRepository,
    ISqlRepository<InsuranceContributionRule> contributionRuleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateInsuranceContributionRuleCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdateInsuranceContributionRuleCommand request,
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

        rule.ContributionType = request.ContributionType.Trim();
        rule.EmployeeRate = request.EmployeeRate;
        rule.EmployerRate = request.EmployerRate;
        rule.CeilingAmount = request.CeilingAmount;
        rule.MinimumAmount = request.MinimumAmount;
        rule.SalaryBasis = request.SalaryBasis.Trim();
        rule.SortOrder = request.SortOrder;
        rule.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new SuccessResponse();
    }
}
