using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Services;

public sealed class ContractExpirationWorker(
    IServiceScopeFactory serviceScopeFactory,
    HrSettings hrSettings,
    ILogger<ContractExpirationWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunExpirationsAsync(stoppingToken);
            // Run check every 24 hours (daily)
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task RunExpirationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var today = GetBusinessToday();
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var contractRepository = scope.ServiceProvider.GetRequiredService<ISqlRepository<EmployeeContract>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Query contracts in Active status where EndDate is in the past
            var expiredContracts = await contractRepository.GetManyByConditionAsync(
                x => x.StatusCode == "Active" && x.EndDate != null && x.EndDate < today,
                null,
                cancellationToken);

            if (expiredContracts.Count == 0)
            {
                return;
            }

            foreach (var contract in expiredContracts)
            {
                logger.LogInformation("Contract Expiration: Expiring contract {ContractNumber} for employee {EmployeeId} (EndDate: {EndDate})",
                    contract.ContractNumber, contract.EmployeeId, contract.EndDate);

                contract.StatusCode = "Expired";
                contract.UpdatedAt = DateTime.UtcNow;
                contract.UpdatedBy = "system";
            }

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (saveResult.IsT1)
            {
                var exception = saveResult.AsT1;
                if (exception is DbUpdateConcurrencyException)
                {
                    logger.LogWarning(exception, "Concurrency conflict occurred while expiring contracts. They will be retried in the next run.");
                }
                else
                {
                    logger.LogError(exception, "An error occurred while saving expired contracts changes.");
                }
            }
            else
            {
                logger.LogInformation("Successfully expired {Count} contracts.", expiredContracts.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing scheduled contract expiration checks.");
        }
    }

    private DateOnly GetBusinessToday()
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(hrSettings.BusinessTimeZone);
            var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            return DateOnly.FromDateTime(localTime.DateTime);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to resolve today's date in business timezone '{BusinessTimeZone}'. Falling back to UTC date.", hrSettings.BusinessTimeZone);
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }
}
