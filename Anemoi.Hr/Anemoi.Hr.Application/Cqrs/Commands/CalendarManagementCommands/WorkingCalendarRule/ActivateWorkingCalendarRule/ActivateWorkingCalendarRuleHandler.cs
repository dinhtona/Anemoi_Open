using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.ActivateWorkingCalendarRule;

public sealed class ActivateWorkingCalendarRuleHandler(
    ISqlRepository<Domain.CalendarManagement.WorkingCalendarRule> workingCalendarRuleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ActivateWorkingCalendarRuleCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        ActivateWorkingCalendarRuleCommand request,
        CancellationToken cancellationToken)
    {
        var workingCalendarRule = await workingCalendarRuleRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (workingCalendarRule is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkingCalendarRuleNotFound);

        if (workingCalendarRule.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkingCalendarRuleAlreadyActive);

        var isOverlapping = await workingCalendarRuleRepository.GetQueryable()
            .AnyAsync(x => x.Id != request.Id && x.IsActive &&
                (workingCalendarRule.EffectiveTo == null || x.EffectiveFrom <= workingCalendarRule.EffectiveTo.Value) &&
                (x.EffectiveTo == null || workingCalendarRule.EffectiveFrom <= x.EffectiveTo.Value),
                cancellationToken);
        if (isOverlapping)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkingCalendarRuleOverlap);

        workingCalendarRule.Activate();

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.WorkingCalendarRuleConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return new SuccessResponse();
    }
}
