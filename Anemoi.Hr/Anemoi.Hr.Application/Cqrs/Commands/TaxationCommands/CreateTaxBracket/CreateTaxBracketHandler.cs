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

        if (request.FromAmount < 0)
        {
            return HrErrorResponses.Create("HR_TAX_BRACKET_INVALID_FROM_AMOUNT");
        }

        if (request.ToAmount.HasValue && request.ToAmount.Value <= request.FromAmount)
        {
            return HrErrorResponses.Create("HR_TAX_BRACKET_INVALID_TO_AMOUNT");
        }

        if (request.Rate < 0)
        {
            return HrErrorResponses.Create("HR_TAX_BRACKET_INVALID_RATE");
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
            return HrErrorResponses.Create("HR_TAX_BRACKET_OVERLAPS");
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
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new CreateTaxBracketResponse { TaxBracketId = newBracket.Id.Value.ToString() };
    }
}
