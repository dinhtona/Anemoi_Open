using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxDeductionRule;

public sealed class UpdateTaxDeductionRuleHandler(
    ISqlRepository<TaxRuleSet> ruleSetRepository,
    ISqlRepository<TaxDeductionRule> deductionRuleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTaxDeductionRuleCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdateTaxDeductionRuleCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var deductionRuleGuid))
        {
            return HrErrorResponses.Create("HR_TAX_DEDUCTION_RULE_NOT_FOUND");
        }

        var deductionRuleId = new TaxDeductionRuleId(deductionRuleGuid);
        var rule = await deductionRuleRepository.GetFirstByConditionAsync(
            x => x.Id == deductionRuleId,
            token: cancellationToken);

        if (rule is null)
        {
            return HrErrorResponses.Create("HR_TAX_DEDUCTION_RULE_NOT_FOUND");
        }

        var ruleSet = await ruleSetRepository.GetFirstByConditionAsync(
            x => x.Id == rule.TaxRuleSetId,
            token: cancellationToken);

        if (ruleSet is null)
        {
            return HrErrorResponses.Create("HR_TAX_RULE_SET_NOT_FOUND");
        }

        if (ruleSet.Status != "Draft")
        {
            return HrErrorResponses.Create("HR_TAX_RULE_SET_NOT_DRAFT");
        }

        if (request.Amount < 0)
        {
            return HrErrorResponses.Create("HR_TAX_DEDUCTION_RULE_INVALID_AMOUNT");
        }

        rule.Amount = request.Amount;
        rule.IsActive = request.IsActive;
        rule.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new SuccessResponse();
    }
}
