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

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxBracket;

public sealed class CreateTaxBracketHandler(
    ISqlRepository<TaxRuleSet> ruleSetRepository,
    ISqlRepository<TaxBracket> bracketRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTaxBracketCommand, OneOf<CreateTaxBracketResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CreateTaxBracketResponse, ErrorDetailResponse>> Handle(
        CreateTaxBracketCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.TaxRuleSetId, out var ruleSetGuid))
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

        // Check overlaps
        var existingBrackets = await bracketRepository.GetManyByConditionAsync(
            x => x.TaxRuleSetId == ruleSetId,
            token: cancellationToken);

        var overlaps = existingBrackets.Any(x =>
            request.FromAmount < (x.ToAmount ?? decimal.MaxValue) &&
            (request.ToAmount ?? decimal.MaxValue) > x.FromAmount);

        if (overlaps)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxBracketOverlaps);
        }

        var newBracket = new TaxBracket
        {
            Id = new TaxBracketId(IdGenerator.NextGuid()),
            TaxRuleSetId = ruleSetId,
            FromAmount = request.FromAmount,
            ToAmount = request.ToAmount,
            Rate = request.Rate,
            QuickDeductionAmount = request.QuickDeductionAmount,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await bracketRepository.CreateOneAsync(newBracket, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return new CreateTaxBracketResponse { TaxBracketId = newBracket.Id.Value.ToString() };
    }
}
