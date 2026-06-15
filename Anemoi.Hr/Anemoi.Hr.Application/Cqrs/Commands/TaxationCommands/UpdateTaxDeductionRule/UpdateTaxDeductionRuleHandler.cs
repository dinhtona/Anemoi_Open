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
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxDeductionRuleNotFound);
        }

        var deductionRuleId = new TaxDeductionRuleId(deductionRuleGuid);
        var rule = await deductionRuleRepository.GetFirstByConditionAsync(
            x => x.Id == deductionRuleId,
            token: cancellationToken);

        if (rule is null)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxDeductionRuleNotFound);
        }

        var ruleSet = await ruleSetRepository.GetFirstByConditionAsync(
            x => x.Id == rule.TaxRuleSetId,
            token: cancellationToken);

        if (ruleSet is null)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetNotFound);
        }

        if (ruleSet.Status != "Draft")
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetNotDraft);
        }

        if (request.Amount < 0)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxDeductionRuleInvalidAmount);
        }

        rule.Amount = request.Amount;
        rule.IsActive = request.IsActive;
        rule.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new SuccessResponse();
    }
}
