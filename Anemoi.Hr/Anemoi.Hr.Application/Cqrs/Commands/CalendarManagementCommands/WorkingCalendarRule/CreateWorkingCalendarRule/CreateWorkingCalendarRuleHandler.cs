using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.CreateWorkingCalendarRule;

public sealed class CreateWorkingCalendarRuleHandler(
    ISqlRepository<Domain.CalendarManagement.WorkingCalendarRule> workingCalendarRuleRepository,
    IUnitOfWork unitOfWork,
    CalendarManagementMapper mapper)
    : ICommandHandler<CreateWorkingCalendarRuleCommand, OneOf<WorkingCalendarRuleIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkingCalendarRuleIdResponse, ErrorDetailResponse>> Handle(
        CreateWorkingCalendarRuleCommand request,
        CancellationToken cancellationToken)
    {
        var isOverlapping = await workingCalendarRuleRepository.GetQueryable()
            .AnyAsync(x => x.IsActive &&
                (request.EffectiveTo == null || x.EffectiveFrom <= request.EffectiveTo.Value) &&
                (x.EffectiveTo == null || request.EffectiveFrom <= x.EffectiveTo.Value),
                cancellationToken);
        if (isOverlapping)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkingCalendarRuleOverlap);

        var workingCalendarRule = Domain.CalendarManagement.WorkingCalendarRule.Create(
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

        await workingCalendarRuleRepository.CreateOneAsync(workingCalendarRule, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.WorkingCalendarRuleConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return mapper.ToWorkingCalendarRuleIdResponse(workingCalendarRule);
    }
}
