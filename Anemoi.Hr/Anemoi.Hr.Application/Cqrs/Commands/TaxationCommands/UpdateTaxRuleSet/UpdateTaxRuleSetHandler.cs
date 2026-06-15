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
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetNotFound);
        }

        var ruleSetId = new TaxRuleSetId(ruleSetGuid);
        var ruleSet = await ruleSetRepository.GetFirstByConditionAsync(
            x => x.Id == ruleSetId,
            token: cancellationToken);

        if (ruleSet is null)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetNotFound);
        }

        if (ruleSet.Status != TaxRuleSetStatusCode.Draft)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetNotDraft);
        }

        var effectiveFrom = request.EffectiveFrom;
        var effectiveTo = request.EffectiveTo;

        if (effectiveTo.HasValue && effectiveFrom > effectiveTo.Value)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetInvalidDateRange);
        }

        // Check for overlaps with other sets (excluding current one)
        var existingSets = await ruleSetRepository.GetManyByConditionAsync(
            x => x.CountryCode == ruleSet.CountryCode && x.TaxType == ruleSet.TaxType && x.Id != ruleSetId && x.Status != TaxRuleSetStatusCode.Inactive,
            token: cancellationToken);

        var overlaps = existingSets.Any(x =>
            effectiveFrom <= (x.EffectiveTo ?? DateOnly.MaxValue) &&
            (effectiveTo ?? DateOnly.MaxValue) >= x.EffectiveFrom);

        if (overlaps)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetOverlappingPeriod);
        }

        ruleSet.Name = request.Name.Trim();
        ruleSet.EffectiveFrom = effectiveFrom;
        ruleSet.EffectiveTo = effectiveTo;
        ruleSet.UpdatedAt = DateTime.UtcNow;
        ruleSet.UpdatedBy = request.UpdatedBy ?? PayrollConstants.SystemActor;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new SuccessResponse();
    }
}
