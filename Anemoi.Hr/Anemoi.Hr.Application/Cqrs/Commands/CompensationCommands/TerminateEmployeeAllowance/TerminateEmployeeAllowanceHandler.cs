using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.TerminateEmployeeAllowance;

public sealed class TerminateEmployeeAllowanceHandler(
    ISqlRepository<EmployeeAllowance> employeeAllowanceRepository,
    HrSettings hrSettings,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TerminateEmployeeAllowanceCommand, OneOf<TerminateEmployeeAllowanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<TerminateEmployeeAllowanceResponse, ErrorDetailResponse>> Handle(
        TerminateEmployeeAllowanceCommand request,
        CancellationToken cancellationToken)
    {
        var today = GetBusinessToday();
        if (request.TerminationDate > today)
            return HrErrorResponses.Create("HR_ALLOWANCE_TERMINATION_DATE_IN_FUTURE");

        var allowance = await employeeAllowanceRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeAllowanceId,
            null,
            cancellationToken);

        if (allowance is null)
            return HrErrorResponses.Create("HR_EMPLOYEE_ALLOWANCE_NOT_FOUND");

        if (request.TerminationDate < allowance.EffectiveFrom)
            return HrErrorResponses.Create("HR_ALLOWANCE_TERMINATION_BEFORE_EFFECTIVE_FROM");

        allowance.EffectiveTo = request.TerminationDate;
        allowance.UpdatedAt = DateTime.UtcNow;
        allowance.UpdatedBy = request.UpdatedBy ?? "system";

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create("HR_ALLOWANCE_CONCURRENCY_CONFLICT")
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return new TerminateEmployeeAllowanceResponse { EmployeeAllowanceId = allowance.Id.Value.ToString() };
    }

    private DateOnly GetBusinessToday()
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(hrSettings.BusinessTimeZone);
            var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            return DateOnly.FromDateTime(localTime.DateTime);
        }
        catch
        {
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }
}
