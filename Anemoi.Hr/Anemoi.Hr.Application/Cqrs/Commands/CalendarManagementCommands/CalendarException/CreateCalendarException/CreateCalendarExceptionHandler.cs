using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CalendarException.CreateCalendarException;

public sealed class CreateCalendarExceptionHandler(
    ISqlRepository<Domain.CalendarManagement.CalendarException> calendarExceptionRepository,
    IUnitOfWork unitOfWork,
    CalendarManagementMapper mapper)
    : ICommandHandler<CreateCalendarExceptionCommand, OneOf<CalendarExceptionIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CalendarExceptionIdResponse, ErrorDetailResponse>> Handle(
        CreateCalendarExceptionCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await calendarExceptionRepository.GetQueryable()
            .AnyAsync(x => x.ExceptionDate == request.ExceptionDate, cancellationToken);
        if (existing)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CalendarExceptionDuplicate);

        var status = request.ExceptionType == CalendarExceptionTypeConstants.WorkingDayOverride
            ? CalendarStatus.WorkingDay
            : CalendarStatus.Holiday;

        var calendarException = Domain.CalendarManagement.CalendarException.Create(
            new CalendarExceptionId(IdGenerator.NextGuid()),
            request.ExceptionDate,
            status,
            request.Name,
            request.RelatedHolidayId is not null ? new PublicHolidayId(request.RelatedHolidayId.Value) : null);

        await calendarExceptionRepository.CreateOneAsync(calendarException, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.CalendarExceptionConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return mapper.ToCalendarExceptionIdResponse(calendarException);
    }
}
