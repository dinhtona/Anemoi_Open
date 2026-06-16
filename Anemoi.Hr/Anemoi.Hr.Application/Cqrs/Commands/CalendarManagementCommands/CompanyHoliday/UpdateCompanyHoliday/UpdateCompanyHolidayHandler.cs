using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CompanyHoliday.UpdateCompanyHoliday;

public sealed class UpdateCompanyHolidayHandler(
    ISqlRepository<Domain.CalendarManagement.CompanyHoliday> companyHolidayRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCompanyHolidayCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdateCompanyHolidayCommand request,
        CancellationToken cancellationToken)
    {
        var companyHoliday = await companyHolidayRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (companyHoliday is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CompanyHolidayNotFound);

        var duplicate = await companyHolidayRepository.GetQueryable()
            .AnyAsync(x => x.HolidayDate == request.HolidayDate && x.Id != request.Id, cancellationToken);
        if (duplicate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CompanyHolidayDuplicate);

        companyHoliday.Update(
            request.HolidayDate,
            request.Name,
            request.Description,
            request.IsRecurringAnnual);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.CompanyHolidayConcurrencyConflict);

        return new SuccessResponse();
    }
}
