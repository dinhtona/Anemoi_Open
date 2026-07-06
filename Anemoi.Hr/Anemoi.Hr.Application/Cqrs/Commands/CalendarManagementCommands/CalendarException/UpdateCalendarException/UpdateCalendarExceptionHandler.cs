using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CalendarException.UpdateCalendarException;

public sealed class UpdateCalendarExceptionHandler(
    ISqlRepository<Domain.CalendarManagement.CalendarException> calendarExceptionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCalendarExceptionCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdateCalendarExceptionCommand request,
        CancellationToken cancellationToken)
    {
        var calendarException = await calendarExceptionRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (calendarException is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CalendarExceptionNotFound);

        var duplicate = await calendarExceptionRepository.GetQueryable()
            .AnyAsync(x => x.ExceptionDate == request.ExceptionDate && x.Id != request.Id, cancellationToken);
        if (duplicate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CalendarExceptionDuplicate);

        var status = request.ExceptionType == CalendarExceptionTypeConstants.WorkingDayOverride
            ? CalendarStatus.WorkingDay
            : CalendarStatus.Holiday;

        calendarException.Update(
            request.ExceptionDate,
            status,
            request.Name,
            request.RelatedHolidayId is not null ? new PublicHolidayId(request.RelatedHolidayId.Value) : null);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.CalendarExceptionConcurrencyConflict);

        return new SuccessResponse();
    }
}
