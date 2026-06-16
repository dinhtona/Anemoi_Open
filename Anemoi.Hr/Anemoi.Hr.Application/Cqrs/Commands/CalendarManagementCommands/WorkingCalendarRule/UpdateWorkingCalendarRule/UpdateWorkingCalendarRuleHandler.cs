using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.UpdateWorkingCalendarRule;

public sealed class UpdateWorkingCalendarRuleHandler(
    ISqlRepository<Domain.CalendarManagement.WorkingCalendarRule> workingCalendarRuleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateWorkingCalendarRuleCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdateWorkingCalendarRuleCommand request,
        CancellationToken cancellationToken)
    {
        var workingCalendarRule = await workingCalendarRuleRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (workingCalendarRule is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkingCalendarRuleNotFound);

        if (workingCalendarRule.IsActive)
        {
            var isOverlapping = await workingCalendarRuleRepository.GetQueryable()
                .AnyAsync(x => x.Id != request.Id && x.IsActive &&
                    (request.EffectiveTo == null || x.EffectiveFrom <= request.EffectiveTo.Value) &&
                    (x.EffectiveTo == null || request.EffectiveFrom <= x.EffectiveTo.Value),
                    cancellationToken);
            if (isOverlapping)
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkingCalendarRuleOverlap);
        }

        workingCalendarRule.Update(
            request.Name,
            request.Description,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.WorkMonday,
            request.WorkTuesday,
            request.WorkWednesday,
            request.WorkThursday,
            request.WorkFriday,
            request.WorkSaturday,
            request.WorkSunday);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.WorkingCalendarRuleConcurrencyConflict);

        return new SuccessResponse();
    }
}
