using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.PublicHoliday.UpdatePublicHoliday;

public sealed class UpdatePublicHolidayHandler(
    ISqlRepository<Domain.CalendarManagement.PublicHoliday> publicHolidayRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePublicHolidayCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdatePublicHolidayCommand request,
        CancellationToken cancellationToken)
    {
        var publicHoliday = await publicHolidayRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (publicHoliday is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PublicHolidayNotFound);

        var duplicate = await publicHolidayRepository.GetQueryable()
            .AnyAsync(x => x.HolidayDate == request.HolidayDate && x.CountryCode == request.CountryCode && x.Id != request.Id, cancellationToken);
        if (duplicate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PublicHolidayDuplicate);

        publicHoliday.Update(
            request.HolidayDate,
            request.Name,
            request.Description,
            request.CountryCode,
            request.IsRecurringAnnual);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.PublicHolidayConcurrencyConflict);

        return new SuccessResponse();
    }
}
