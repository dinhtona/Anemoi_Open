using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.DeactivateInsuranceRuleSet;

public sealed class DeactivateInsuranceRuleSetHandler(
    ISqlRepository<InsuranceRuleSet> ruleSetRepository,
    ISqlRepository<InsuranceAuditLog> auditLogRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateInsuranceRuleSetCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeactivateInsuranceRuleSetCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var ruleSetGuid))
        {
            return HrErrorResponses.Create("HR_INSURANCE_RULE_SET_NOT_FOUND");
        }

        var ruleSetId = new InsuranceRuleSetId(ruleSetGuid);
        var ruleSet = await ruleSetRepository.GetFirstByConditionAsync(
            x => x.Id == ruleSetId,
            token: cancellationToken);

        if (ruleSet is null)
        {
            return HrErrorResponses.Create("HR_INSURANCE_RULE_SET_NOT_FOUND");
        }

        if (ruleSet.Status != InsuranceRuleSetStatuses.Active)
        {
            return HrErrorResponses.Create("HR_INSURANCE_RULE_SET_NOT_ACTIVE");
        }

        ruleSet.Status = InsuranceRuleSetStatuses.Inactive;
        ruleSet.UpdatedAt = DateTime.UtcNow;
        ruleSet.UpdatedBy = request.DeactivatedBy ?? "system";

        var auditLog = new InsuranceAuditLog
        {
            Id = new InsuranceAuditLogId(IdGenerator.NextGuid()),
            RuleSetId = ruleSetId,
            Action = "Deactivate",
            Description = $"Insurance rule set '{ruleSet.Name}' deactivated",
            PerformedBy = request.DeactivatedBy ?? "system",
            PerformedAt = DateTime.UtcNow
        };

        await auditLogRepository.CreateOneAsync(auditLog, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new SuccessResponse();
    }
}
