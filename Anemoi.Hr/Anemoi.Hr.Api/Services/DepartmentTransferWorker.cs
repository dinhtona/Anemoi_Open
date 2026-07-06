using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Anemoi.Hr.Api.Services;

public sealed class DepartmentTransferWorker(
    IServiceScopeFactory serviceScopeFactory,
    HrSettings hrSettings,
    ILogger<DepartmentTransferWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunTransfersAsync(stoppingToken);
            // Run check every 1 hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task RunTransfersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var today = GetBusinessToday();
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var historyRepository = scope.ServiceProvider.GetRequiredService<ISqlRepository<EmployeeDepartmentHistory>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Query only records that actually need synchronization, including their Employee entity
            var dbQuery = historyRepository.GetQueryable();
            var pendingSyncs = await dbQuery
                .Include(x => x.Employee)
                .Where(x => x.IsPrimary 
                            && x.EffectiveFrom <= today 
                            && (x.EffectiveTo == null || x.EffectiveTo >= today)
                            && x.Employee.PrimaryDepartmentId != x.DepartmentId)
                .ToListAsync(cancellationToken);

            if (pendingSyncs.Count == 0)
            {
                return;
            }

            foreach (var history in pendingSyncs)
            {
                if (history.Employee is not null)
                {
                    logger.LogInformation("Scheduled sync: Updating employee {EmployeeId} department to {DepartmentId} as of effective date {EffectiveFrom}", 
                        history.Employee.Id, history.DepartmentId, history.EffectiveFrom);
                    history.Employee.PrimaryDepartmentId = history.DepartmentId;
                    history.Employee.UpdatedAt = DateTime.UtcNow;
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing scheduled department transfers.");
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
