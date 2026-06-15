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

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.DeleteTaxBracket;

public sealed class DeleteTaxBracketHandler(
    ISqlRepository<TaxRuleSet> ruleSetRepository,
    ISqlRepository<TaxBracket> bracketRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTaxBracketCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeleteTaxBracketCommand request,
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

        await bracketRepository.RemoveOneAsync(bracket, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new SuccessResponse();
    }
}
