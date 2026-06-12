using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxDeductionRule;

public sealed class CreateTaxDeductionRuleHandler(
    ISqlRepository<TaxRuleSet> ruleSetRepository,
    ISqlRepository<TaxDeductionRule> deductionRuleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTaxDeductionRuleCommand, OneOf<CreateTaxDeductionRuleResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CreateTaxDeductionRuleResponse, ErrorDetailResponse>> Handle(
        CreateTaxDeductionRuleCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.TaxRuleSetId, out var ruleSetGuid))
        {
            return HrErrorResponses.Create("HR_TAX_RULE_SET_NOT_FOUND");
        }

        var ruleSetId = new TaxRuleSetId(ruleSetGuid);
        var ruleSet = await ruleSetRepository.GetFirstByConditionAsync(
            x => x.Id == ruleSetId,
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

        var cleanType = request.DeductionType.Trim();

        // Check if DeductionType already exists in this rule set
        var exists = await deductionRuleRepository.ExistByConditionAsync(
            x => x.TaxRuleSetId == ruleSetId && x.DeductionType == cleanType,
            token: cancellationToken);

        if (exists)
        {
            return HrErrorResponses.Create("HR_TAX_DEDUCTION_RULE_TYPE_ALREADY_EXISTS");
        }

        var newRule = new TaxDeductionRule
        {
            Id = new TaxDeductionRuleId(IdGenerator.NextGuid()),
            TaxRuleSetId = ruleSetId,
            DeductionType = cleanType,
            Amount = request.Amount,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await deductionRuleRepository.CreateOneAsync(newRule, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new CreateTaxDeductionRuleResponse { TaxDeductionRuleId = newRule.Id.Value.ToString() };
    }
}
