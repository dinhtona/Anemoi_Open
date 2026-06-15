using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
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

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.ActivateTaxRuleSet;

public sealed class ActivateTaxRuleSetHandler(
    ISqlRepository<TaxRuleSet> ruleSetRepository,
    ISqlRepository<TaxBracket> bracketRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ActivateTaxRuleSetCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        ActivateTaxRuleSetCommand request,
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

        if (ruleSet.Status == "Active")
        {
            return new SuccessResponse(); // Already active
        }

        // Check if there are brackets defined
        var hasBrackets = await bracketRepository.ExistByConditionAsync(
            x => x.TaxRuleSetId == ruleSetId,
            token: cancellationToken);

        if (!hasBrackets)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetHasNoBrackets);
        }

        // Check for overlaps with other ACTIVE sets
        var activeSets = await ruleSetRepository.GetManyByConditionAsync(
            x => x.CountryCode == ruleSet.CountryCode && x.TaxType == ruleSet.TaxType && x.Id != ruleSetId && x.Status == "Active",
            token: cancellationToken);

        var overlaps = activeSets.Any(x =>
            ruleSet.EffectiveFrom <= (x.EffectiveTo ?? DateOnly.MaxValue) &&
            (ruleSet.EffectiveTo ?? DateOnly.MaxValue) >= x.EffectiveFrom);

        if (overlaps)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetOverlapWithActive);
        }

        ruleSet.Status = "Active";
        ruleSet.UpdatedAt = DateTime.UtcNow;
        ruleSet.UpdatedBy = request.UpdatedBy ?? PayrollConstants.SystemActor;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new SuccessResponse();
    }
}
