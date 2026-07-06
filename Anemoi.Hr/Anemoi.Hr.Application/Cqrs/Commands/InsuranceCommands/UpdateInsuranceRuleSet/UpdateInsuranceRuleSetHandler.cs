using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.UpdateInsuranceRuleSet;

public sealed class UpdateInsuranceRuleSetHandler(
    ISqlRepository<InsuranceRuleSet> ruleSetRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateInsuranceRuleSetCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdateInsuranceRuleSetCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var ruleSetGuid))
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InsuranceRuleSetNotFound);
        }

        var ruleSetId = new InsuranceRuleSetId(ruleSetGuid);
        var ruleSet = await ruleSetRepository.GetFirstByConditionAsync(
            x => x.Id == ruleSetId,
            token: cancellationToken);

        if (ruleSet is null)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InsuranceRuleSetNotFound);
        }

        if (ruleSet.Status != InsuranceRuleSetStatuses.Draft)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InsuranceRuleSetNotDraft);
        }

        ruleSet.Name = request.Name.Trim();
        ruleSet.Currency = request.Currency.Trim().ToUpperInvariant();
        ruleSet.EffectiveFrom = request.EffectiveFrom;
        ruleSet.EffectiveTo = request.EffectiveTo;
        ruleSet.UpdatedAt = DateTime.UtcNow;
        ruleSet.UpdatedBy = request.UpdatedBy ?? PayrollConstants.SystemActor;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return new SuccessResponse();
    }
}
