using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Events;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeSalary;

public sealed class ChangeSalaryHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<SalaryGrade> salaryGradeRepository,
    ISqlRepository<EmployeeSalary> employeeSalaryRepository,
    ISqlRepository<SalaryValidationBypassLog> salaryValidationBypassLogRepository,
    IMediator mediator,
    HrSettings hrSettings,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeSalaryCommand, OneOf<ChangeSalaryResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<ChangeSalaryResponse, ErrorDetailResponse>> Handle(
        ChangeSalaryCommand request,
        CancellationToken cancellationToken)
    {
        var today = GetBusinessToday();

        if (request.EffectiveFrom > today)
            return HrErrorResponses.Create("HR_SALARY_EFFECTIVE_DATE_IN_FUTURE");

        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId,
            null,
            cancellationToken);

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        if (request.EffectiveFrom < employee.JoinDate)
            return HrErrorResponses.Create("HR_SALARY_DATE_BEFORE_JOIN_DATE");

        // Load existing salaries to enforce sequential appends
        var histories = await employeeSalaryRepository.GetManyByConditionAsync(
            x => x.EmployeeId == request.EmployeeId,
            null,
            cancellationToken);

        var latestSalary = histories.OrderByDescending(x => x.EffectiveFrom).FirstOrDefault();
        if (latestSalary is not null)
        {
            if (request.EffectiveFrom <= latestSalary.EffectiveFrom)
                return HrErrorResponses.Create("HR_SALARY_TIMELINE_NOT_SEQUENTIAL");
        }

        // Fetch salary grade and check boundaries if a range exists
        var salaryGrade = await salaryGradeRepository.GetFirstByConditionAsync(
            x => x.GradeCode == employee.GradeCode && x.IsActive,
            q => q.Include(g => g.Ranges),
            cancellationToken);

        SalaryRange activeRange = null;
        if (salaryGrade is not null)
        {
            activeRange = salaryGrade.Ranges.FirstOrDefault(r => 
                r.IsActive && 
                string.Equals(r.Currency, request.Currency, StringComparison.OrdinalIgnoreCase) && 
                r.EffectiveFrom <= request.EffectiveFrom && 
                (r.EffectiveTo == null || r.EffectiveTo >= request.EffectiveFrom));
        }

        bool validationBypassed = false;
        string warningCode = null;

        if (activeRange is not null)
        {
            if (request.BaseSalary < activeRange.MinSalary || request.BaseSalary > activeRange.MaxSalary)
                return HrErrorResponses.Create("HR_SALARY_OUT_OF_GRADE_RANGE");
        }
        else
        {
            // Range validation bypassed
            validationBypassed = true;
            warningCode = salaryGrade is null ? "SALARY_GRADE_NOT_CONFIGURED" : "SALARY_RANGE_NOT_CONFIGURED";

            var bypassLog = new SalaryValidationBypassLog
            {
                Id = new SalaryValidationBypassLogId(IdGenerator.NextGuid()),
                EmployeeId = request.EmployeeId,
                GradeCode = employee.GradeCode ?? "NONE",
                RequestedSalary = request.BaseSalary,
                Currency = request.Currency,
                BypassReason = warningCode,
                CreatedBy = request.CreatedBy ?? "system",
                CreatedAt = DateTime.UtcNow
            };

            await salaryValidationBypassLogRepository.CreateOneAsync(bypassLog, cancellationToken);

            var bypassEvent = new SalaryValidationBypassedEvent(
                request.EmployeeId.Value,
                employee.GradeCode ?? "NONE",
                request.BaseSalary,
                request.Currency,
                warningCode,
                request.CreatedBy ?? "system",
                DateTime.UtcNow
            );

            await mediator.Publish(bypassEvent, cancellationToken);
        }

        // Close previous latest salary
        if (latestSalary is not null)
        {
            latestSalary.EffectiveTo = request.EffectiveFrom.AddDays(-1);
        }

        // Create new salary history record
        var newSalary = new EmployeeSalary
        {
            Id = new EmployeeSalaryId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            SalaryGradeId = salaryGrade?.Id,
            GradeCodeSnapshot = employee.GradeCode ?? "NONE",
            BaseSalary = request.BaseSalary,
            SalaryType = request.SalaryType,
            Currency = request.Currency,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = null,
            Reason = request.Reason,
            CreatedBy = request.CreatedBy ?? "system",
            CreatedAt = DateTime.UtcNow
        };

        await employeeSalaryRepository.CreateOneAsync(newSalary, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException ||
                   CompensationPersistenceErrors.IsUniqueConstraintViolation(saveResult.AsT1)
                ? HrErrorResponses.Create("HR_SALARY_CONCURRENCY_CONFLICT")
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return new ChangeSalaryResponse
        {
            EmployeeSalaryId = newSalary.Id.Value.ToString(),
            ValidationBypassed = validationBypassed,
            WarningCode = warningCode
        };
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
