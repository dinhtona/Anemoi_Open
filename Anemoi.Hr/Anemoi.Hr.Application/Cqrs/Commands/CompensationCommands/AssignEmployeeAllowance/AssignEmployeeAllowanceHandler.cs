using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.AssignEmployeeAllowance;

public sealed class AssignEmployeeAllowanceHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<AllowanceType> allowanceTypeRepository,
    ISqlRepository<EmployeeAllowance> employeeAllowanceRepository,
    HrSettings hrSettings,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignEmployeeAllowanceCommand, OneOf<AssignEmployeeAllowanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AssignEmployeeAllowanceResponse, ErrorDetailResponse>> Handle(
        AssignEmployeeAllowanceCommand request,
        CancellationToken cancellationToken)
    {
        var today = GetBusinessToday();
        if (request.EffectiveFrom > today)
            return HrErrorResponses.Create("HR_ALLOWANCE_EFFECTIVE_DATE_IN_FUTURE");

        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId,
            null,
            cancellationToken);

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        if (request.EffectiveFrom < employee.JoinDate)
            return HrErrorResponses.Create("HR_ALLOWANCE_DATE_BEFORE_JOIN_DATE");

        var typeExists = await allowanceTypeRepository.ExistByConditionAsync(
            x => x.Id == request.AllowanceTypeId && x.IsActive,
            cancellationToken);
        if (!typeExists)
            return HrErrorResponses.Create("HR_ALLOWANCE_TYPE_NOT_FOUND");

        // Fetch existing allowances for this type and currency
        var existingAllowances = await employeeAllowanceRepository.GetManyByConditionAsync(
            x => x.EmployeeId == request.EmployeeId &&
                 x.AllowanceTypeId == request.AllowanceTypeId &&
                 x.Currency == request.Currency,
            null,
            cancellationToken);

        // Check for duplicate EffectiveFrom
        if (existingAllowances.Any(x => x.EffectiveFrom == request.EffectiveFrom))
            return HrErrorResponses.Create("HR_ALLOWANCE_TIMELINE_DUPLICATE_DATE");

        // Find active allowance at request.EffectiveFrom
        var activeAllowance = existingAllowances.FirstOrDefault(x =>
            x.EffectiveFrom <= request.EffectiveFrom &&
            (x.EffectiveTo == null || x.EffectiveTo >= request.EffectiveFrom));

        if (activeAllowance is not null)
        {
            if (request.EffectiveFrom <= activeAllowance.EffectiveFrom)
                return HrErrorResponses.Create("HR_ALLOWANCE_TIMELINE_NOT_SEQUENTIAL");

            // Close the currently active allowance
            activeAllowance.EffectiveTo = request.EffectiveFrom.AddDays(-1);
            activeAllowance.UpdatedAt = DateTime.UtcNow;
            activeAllowance.UpdatedBy = request.CreatedBy ?? "system";
        }

        // Verify no future allowances overlap
        var hasFutureOverlap = existingAllowances.Any(x => x.EffectiveFrom > request.EffectiveFrom);
        if (hasFutureOverlap)
            return HrErrorResponses.Create("HR_ALLOWANCE_TIMELINE_NOT_SEQUENTIAL");

        var newAllowance = new EmployeeAllowance
        {
            Id = new EmployeeAllowanceId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            AllowanceTypeId = request.AllowanceTypeId,
            Amount = request.Amount,
            Currency = request.Currency.Trim(),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            CreatedBy = request.CreatedBy ?? "system",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = request.CreatedBy ?? "system",
            UpdatedAt = DateTime.UtcNow
        };

        await employeeAllowanceRepository.CreateOneAsync(newAllowance, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException ||
                   CompensationPersistenceErrors.IsUniqueConstraintViolation(saveResult.AsT1)
                ? HrErrorResponses.Create("HR_ALLOWANCE_CONCURRENCY_CONFLICT")
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return new AssignEmployeeAllowanceResponse { EmployeeAllowanceId = newAllowance.Id.Value.ToString() };
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
