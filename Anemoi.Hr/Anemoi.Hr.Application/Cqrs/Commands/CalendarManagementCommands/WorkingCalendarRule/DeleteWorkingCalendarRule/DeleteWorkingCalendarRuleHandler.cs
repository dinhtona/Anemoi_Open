using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.DeleteWorkingCalendarRule;

public sealed class DeleteWorkingCalendarRuleHandler(
    ISqlRepository<Domain.CalendarManagement.WorkingCalendarRule> workingCalendarRuleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteWorkingCalendarRuleCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeleteWorkingCalendarRuleCommand request,
        CancellationToken cancellationToken)
    {
        var workingCalendarRule = await workingCalendarRuleRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (workingCalendarRule is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkingCalendarRuleNotFound);

        await workingCalendarRuleRepository.RemoveOneAsync(workingCalendarRule, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.WorkingCalendarRuleConcurrencyConflict);

        return new SuccessResponse();
    }
}
