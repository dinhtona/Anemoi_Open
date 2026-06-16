using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.PublicHoliday.DeletePublicHoliday;

public sealed class DeletePublicHolidayHandler(
    ISqlRepository<Domain.CalendarManagement.PublicHoliday> publicHolidayRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePublicHolidayCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeletePublicHolidayCommand request,
        CancellationToken cancellationToken)
    {
        var publicHoliday = await publicHolidayRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (publicHoliday is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PublicHolidayNotFound);

        await publicHolidayRepository.RemoveOneAsync(publicHoliday, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.PublicHolidayConcurrencyConflict);

        return new SuccessResponse();
    }
}
