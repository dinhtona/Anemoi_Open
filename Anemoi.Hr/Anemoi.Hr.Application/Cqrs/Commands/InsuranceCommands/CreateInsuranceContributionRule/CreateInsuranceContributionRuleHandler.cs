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
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CreateInsuranceContributionRule;

public sealed class CreateInsuranceContributionRuleHandler(
    ISqlRepository<InsuranceRuleSet> ruleSetRepository,
    ISqlRepository<InsuranceContributionRule> contributionRuleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateInsuranceContributionRuleCommand, OneOf<CreateInsuranceContributionRuleResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CreateInsuranceContributionRuleResponse, ErrorDetailResponse>> Handle(
        CreateInsuranceContributionRuleCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.RuleSetId, out var ruleSetGuid))
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InsuranceRuleSetNotFound);
        }

        var ruleSetId = new InsuranceRuleSetId(ruleSetGuid);
        var ruleSet = await ruleSetRepository.GetFirstByConditionAsync(
            x => x.Id == ruleSetId,
            token: cancellationToken);

        if (ruleSet is null)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InsuranceRuleSetNotFound);
        }

        if (ruleSet.Status != InsuranceRuleSetStatuses.Draft)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InsuranceRuleSetNotDraft);
        }

        var newRule = new InsuranceContributionRule
        {
            Id = new InsuranceContributionRuleId(IdGenerator.NextGuid()),
            RuleSetId = ruleSetId,
            ContributionType = request.ContributionType.Trim(),
            EmployeeRate = request.EmployeeRate,
            EmployerRate = request.EmployerRate,
            CeilingAmount = request.CeilingAmount,
            MinimumAmount = request.MinimumAmount,
            SalaryBasis = request.SalaryBasis.Trim(),
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await contributionRuleRepository.CreateOneAsync(newRule, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new CreateInsuranceContributionRuleResponse { InsuranceContributionRuleId = newRule.Id.Value.ToString() };
    }
}
