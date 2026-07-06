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

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.PublicHoliday.CreatePublicHoliday;

public sealed class CreatePublicHolidayHandler(
    ISqlRepository<Domain.CalendarManagement.PublicHoliday> publicHolidayRepository,
    IUnitOfWork unitOfWork,
    CalendarManagementMapper mapper)
    : ICommandHandler<CreatePublicHolidayCommand, OneOf<PublicHolidayIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PublicHolidayIdResponse, ErrorDetailResponse>> Handle(
        CreatePublicHolidayCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await publicHolidayRepository.GetQueryable()
            .AnyAsync(x => x.HolidayDate == request.HolidayDate && x.CountryCode == request.CountryCode, cancellationToken);
        if (existing)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PublicHolidayDuplicate);

        var publicHoliday = Domain.CalendarManagement.PublicHoliday.Create(
            new PublicHolidayId(IdGenerator.NextGuid()),
            request.HolidayDate,
            request.Name,
            request.Description,
            request.CountryCode,
            request.IsRecurringAnnual);

        await publicHolidayRepository.CreateOneAsync(publicHoliday, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.PublicHolidayConcurrencyConflict);

        return mapper.ToPublicHolidayIdResponse(publicHoliday);
    }
}
