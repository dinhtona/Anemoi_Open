using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CompanyHoliday.DeleteCompanyHoliday;

public sealed class DeleteCompanyHolidayHandler(
    ISqlRepository<Domain.CalendarManagement.CompanyHoliday> companyHolidayRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCompanyHolidayCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeleteCompanyHolidayCommand request,
        CancellationToken cancellationToken)
    {
        var companyHoliday = await companyHolidayRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (companyHoliday is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CompanyHolidayNotFound);

        await companyHolidayRepository.RemoveOneAsync(companyHoliday, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.CompanyHolidayConcurrencyConflict);

        return new SuccessResponse();
    }
}
