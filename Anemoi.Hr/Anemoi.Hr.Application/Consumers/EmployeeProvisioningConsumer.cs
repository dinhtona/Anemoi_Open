using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Contract.Identity.Events;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Anemoi.Hr.Application.Consumers;

public sealed class EmployeeProvisioningConsumer(
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IConsumer<UserProvisionedForEmployeeIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserProvisionedForEmployeeIntegrationEvent> context)
    {
        var message = context.Message;

        var employee = await employeeRepository
            .GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == new EmployeeId(message.EmployeeId), context.CancellationToken);

        if (employee is null)
        {
            logger.Warning(
                "[EmployeeProvisioning] Employee {EmployeeId} not found, skipping link",
                message.EmployeeId);
            return;
        }

        if (employee.IdentityUserId is not null)
        {
            logger.Information(
                "[EmployeeProvisioning] Employee {EmployeeCode} already linked to user {UserId}, skipping",
                employee.EmployeeCode, employee.IdentityUserId);
            return;
        }

        employee.IdentityUserId = message.UserId;
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.Information(
            "[EmployeeProvisioning] Linked employee {EmployeeCode} to user {UserId}",
            employee.EmployeeCode, message.UserId);
    }
}
