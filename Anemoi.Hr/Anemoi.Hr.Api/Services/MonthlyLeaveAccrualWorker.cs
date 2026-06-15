using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Events;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Anemoi.Hr.Api.Services;

public sealed class MonthlyLeaveAccrualWorker(
    IServiceScopeFactory serviceScopeFactory,
    HrSettings hrSettings,
    ILogger<MonthlyLeaveAccrualWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunAccrualAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(Math.Max(1, hrSettings.MonthlyAccrualCheckIntervalHours)),
                stoppingToken);
        }
    }

    private async Task RunAccrualAsync(CancellationToken cancellationToken)
    {
        var businessNow = GetBusinessNow();
        var today = DateOnly.FromDateTime(businessNow.DateTime);
        if (businessNow.Hour != hrSettings.MonthlyAccrualRunHour ||
            today.Day != DateTime.DaysInMonth(today.Year, today.Month))
        {
            return;
        }

        var yearMonth = $"{today.Year:D4}-{today.Month:D2}";
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var employees = scope.ServiceProvider.GetRequiredService<ISqlRepository<Employee>>();
        var policies = scope.ServiceProvider.GetRequiredService<ISqlRepository<LeavePolicy>>();
        var balances = scope.ServiceProvider.GetRequiredService<ISqlRepository<LeaveBalance>>();
        var accrualRuns = scope.ServiceProvider.GetRequiredService<ISqlRepository<LeaveAccrualRun>>();
        var transactions = scope.ServiceProvider.GetRequiredService<ISqlRepository<LeaveTransaction>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var activeEmployees = await employees.GetManyByConditionAsync(
            x => x.EmploymentStatusCode == EmploymentStatusCode.Active,
            null,
            cancellationToken);
        var activePolicies = await policies.GetManyByConditionAsync(x => x.IsActive, null, cancellationToken);

        foreach (var employee in activeEmployees)
        {
            foreach (var policy in activePolicies)
            {
                var alreadyRun = await accrualRuns.ExistByConditionAsync(
                    x => x.EmployeeId == employee.Id && x.LeavePolicyId == policy.Id && x.YearMonth == yearMonth,
                    cancellationToken);
                if (alreadyRun) continue;

                var balance = await balances.GetFirstByConditionAsync(
                    x => x.EmployeeId == employee.Id && x.LeavePolicyId == policy.Id && x.Year == today.Year,
                    null,
                    cancellationToken);
                if (balance is null)
                {
                    balance = new LeaveBalance
                    {
                        Id = new LeaveBalanceId(IdGenerator.NextGuid()),
                        EmployeeId = employee.Id,
                        LeavePolicyId = policy.Id,
                        Year = today.Year,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await balances.CreateOneAsync(balance, cancellationToken);
                }

                balance.AccruedDays += policy.MonthlyAccrualDays;
                balance.RemainingDays += policy.MonthlyAccrualDays;
                balance.UpdatedAt = DateTime.UtcNow;
                await transactions.CreateOneAsync(new LeaveTransaction
                {
                    Id = new LeaveTransactionId(IdGenerator.NextGuid()),
                    EmployeeId = employee.Id,
                    LeavePolicyId = policy.Id,
                    LeaveBalanceId = balance.Id,
                    TransactionTypeCode = "Accrual",
                    Days = policy.MonthlyAccrualDays,
                    BalanceAfterDays = balance.RemainingDays,
                    SourceType = "LeaveAccrualRun",
                    SourceId = yearMonth,
                    Reason = yearMonth,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
                await accrualRuns.CreateOneAsync(new LeaveAccrualRun
                {
                    Id = new LeaveAccrualRunId(IdGenerator.NextGuid()),
                    EmployeeId = employee.Id,
                    LeavePolicyId = policy.Id,
                    YearMonth = yearMonth,
                    AccruedDays = policy.MonthlyAccrualDays,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
            }
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            logger.LogError("Monthly leave accrual failed for {YearMonth}", yearMonth);
            return;
        }

        foreach (var employee in activeEmployees)
        {
            var employeeBalances = await balances.GetManyByConditionAsync(
                x => x.EmployeeId == employee.Id && x.Year == today.Year,
                null,
                cancellationToken);
            foreach (var balance in employeeBalances)
            {
                await publishEndpoint.Publish(new LeaveBalanceChangedIntegrationEvent(
                    balance.EmployeeId.Value.ToString(),
                    balance.LeavePolicyId.Value.ToString(),
                    balance.Year,
                    balance.RemainingDays,
                    "Accrual"), cancellationToken);
            }
        }
    }

    private DateTimeOffset GetBusinessNow()
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(hrSettings.BusinessTimeZone);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        }
        catch (TimeZoneNotFoundException exception)
        {
            logger.LogError(exception, "HR business timezone {BusinessTimeZone} was not found.",
                hrSettings.BusinessTimeZone);
            return DateTimeOffset.UtcNow;
        }
        catch (InvalidTimeZoneException exception)
        {
            logger.LogError(exception, "HR business timezone {BusinessTimeZone} is invalid.",
                hrSettings.BusinessTimeZone);
            return DateTimeOffset.UtcNow;
        }
    }
}
