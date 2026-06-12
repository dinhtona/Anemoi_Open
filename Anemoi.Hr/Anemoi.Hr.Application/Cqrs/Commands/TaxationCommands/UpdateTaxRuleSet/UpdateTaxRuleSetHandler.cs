using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxRuleSet;

public sealed class UpdateTaxRuleSetHandler(
    ISqlRepository<TaxRuleSet> ruleSetRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTaxRuleSetCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdateTaxRuleSetCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var ruleSetGuid))
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

        var effectiveFrom = request.EffectiveFrom;
        var effectiveTo = request.EffectiveTo;

        if (effectiveTo.HasValue && effectiveFrom > effectiveTo.Value)
        {
            return HrErrorResponses.Create("HR_TAX_RULE_SET_INVALID_DATE_RANGE");
        }

        // Check for overlaps with other sets (excluding current one)
        var existingSets = await ruleSetRepository.GetManyByConditionAsync(
            x => x.CountryCode == ruleSet.CountryCode && x.TaxType == ruleSet.TaxType && x.Id != ruleSetId && x.Status != "Inactive",
            token: cancellationToken);

        var overlaps = existingSets.Any(x =>
            effectiveFrom <= (x.EffectiveTo ?? DateOnly.MaxValue) &&
            (effectiveTo ?? DateOnly.MaxValue) >= x.EffectiveFrom);

        if (overlaps)
        {
            return HrErrorResponses.Create("HR_TAX_RULE_SET_OVERLAPPING_PERIOD");
        }

        ruleSet.Name = request.Name.Trim();
        ruleSet.EffectiveFrom = effectiveFrom;
        ruleSet.EffectiveTo = effectiveTo;
        ruleSet.UpdatedAt = DateTime.UtcNow;
        ruleSet.UpdatedBy = request.UpdatedBy ?? "system";

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new SuccessResponse();
    }
}
