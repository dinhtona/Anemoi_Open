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

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxBracket;

public sealed class UpdateTaxBracketHandler(
    ISqlRepository<TaxRuleSet> ruleSetRepository,
    ISqlRepository<TaxBracket> bracketRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTaxBracketCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdateTaxBracketCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var bracketGuid))
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxBracketNotFound);
        }

        var bracketId = new TaxBracketId(bracketGuid);
        var bracket = await bracketRepository.GetFirstByConditionAsync(
            x => x.Id == bracketId,
            token: cancellationToken);

        if (bracket is null)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxBracketNotFound);
        }

        var ruleSet = await ruleSetRepository.GetFirstByConditionAsync(
            x => x.Id == bracket.TaxRuleSetId,
            token: cancellationToken);

        if (ruleSet is null)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetNotFound);
        }

        if (ruleSet.Status != "Draft")
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetNotDraft);
        }

        if (request.FromAmount < 0)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxBracketInvalidFromAmount);
        }

        if (request.ToAmount.HasValue && request.ToAmount.Value <= request.FromAmount)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxBracketInvalidToAmount);
        }

        if (request.Rate < 0)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxBracketInvalidRate);
        }

        // Check overlaps with other brackets in the same set
        var existingBrackets = await bracketRepository.GetManyByConditionAsync(
            x => x.TaxRuleSetId == bracket.TaxRuleSetId && x.Id != bracketId,
            token: cancellationToken);

        var overlaps = existingBrackets.Any(x =>
            request.FromAmount < (x.ToAmount ?? decimal.MaxValue) &&
            (request.ToAmount ?? decimal.MaxValue) > x.FromAmount);

        if (overlaps)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxBracketOverlaps);
        }

        bracket.FromAmount = request.FromAmount;
        bracket.ToAmount = request.ToAmount;
        bracket.Rate = request.Rate;
        bracket.QuickDeductionAmount = request.QuickDeductionAmount;
        bracket.SortOrder = request.SortOrder;
        bracket.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new SuccessResponse();
    }
}
