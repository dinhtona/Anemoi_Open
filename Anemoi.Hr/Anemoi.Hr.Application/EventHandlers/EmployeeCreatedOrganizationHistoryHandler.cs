using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Employees.Events;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;

namespace Anemoi.Hr.Application.EventHandlers;

public sealed class EmployeeCreatedOrganizationHistoryHandler(
    ISqlRepository<Employee> employeeRepo,
    ISqlRepository<EmployeeOrganizationHistory> orgHistoryRepo,
    IUnitOfWork unitOfWork)
    : INotificationHandler<EmployeeCreatedDomainEvent>
{
    public async Task Handle(EmployeeCreatedDomainEvent notification, CancellationToken ct)
    {
        var employee = await employeeRepo.GetFirstByConditionAsync(
            x => x.Id == notification.EmployeeId, null, ct);
        if (employee == null) return;

        var history = new EmployeeOrganizationHistory
        {
            Id = new EmployeeOrganizationHistoryId(IdGenerator.NextGuid()),
            EmployeeId = employee.Id,
            DepartmentId = employee.PrimaryDepartmentId,
            PositionId = employee.PrimaryPositionId,
            ManagerEmployeeId = employee.DirectManagerEmployeeId,
            GradeCode = employee.GradeCode,
            EffectiveDate = employee.JoinDate,
            ChangeReasonCode = "Initial"
        };
        await orgHistoryRepo.CreateOneAsync(history, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
