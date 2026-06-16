using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CalendarException.DeleteCalendarException;

public sealed class DeleteCalendarExceptionHandler(
    ISqlRepository<Domain.CalendarManagement.CalendarException> calendarExceptionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCalendarExceptionCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeleteCalendarExceptionCommand request,
        CancellationToken cancellationToken)
    {
        var calendarException = await calendarExceptionRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (calendarException is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CalendarExceptionNotFound);

        await calendarExceptionRepository.RemoveOneAsync(calendarException, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.CalendarExceptionConcurrencyConflict);

        return new SuccessResponse();
    }
}
