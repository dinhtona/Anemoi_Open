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

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.DeactivateTaxRuleSet;

public sealed class DeactivateTaxRuleSetHandler(
    ISqlRepository<TaxRuleSet> ruleSetRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateTaxRuleSetCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeactivateTaxRuleSetCommand request,
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

        ruleSet.Status = "Inactive";
        ruleSet.UpdatedAt = DateTime.UtcNow;
        ruleSet.UpdatedBy = request.UpdatedBy ?? "system";

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new SuccessResponse();
    }
}
