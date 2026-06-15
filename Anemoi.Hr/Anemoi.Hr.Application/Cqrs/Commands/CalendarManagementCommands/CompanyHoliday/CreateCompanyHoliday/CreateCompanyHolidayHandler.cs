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

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CompanyHoliday.CreateCompanyHoliday;

public sealed class CreateCompanyHolidayHandler(
    ISqlRepository<Domain.CalendarManagement.CompanyHoliday> companyHolidayRepository,
    IUnitOfWork unitOfWork,
    CalendarManagementMapper mapper)
    : ICommandHandler<CreateCompanyHolidayCommand, OneOf<CompanyHolidayIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CompanyHolidayIdResponse, ErrorDetailResponse>> Handle(
        CreateCompanyHolidayCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await companyHolidayRepository.GetQueryable()
            .AnyAsync(x => x.HolidayDate == request.HolidayDate, cancellationToken);
        if (existing)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CompanyHolidayDuplicate);

        var companyHoliday = Domain.CalendarManagement.CompanyHoliday.Create(
            new CompanyHolidayId(IdGenerator.NextGuid()),
            request.HolidayDate,
            request.Name,
            request.Description,
            request.IsRecurringAnnual);

        await companyHolidayRepository.CreateOneAsync(companyHoliday, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.CompanyHolidayConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return mapper.ToCompanyHolidayIdResponse(companyHoliday);
    }
}
