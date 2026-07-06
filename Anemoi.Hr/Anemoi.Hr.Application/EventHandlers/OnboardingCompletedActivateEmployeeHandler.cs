using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Onboarding.Events;
using MediatR;

namespace Anemoi.Hr.Application.EventHandlers;

public sealed class OnboardingCompletedActivateEmployeeHandler(
    ISqlRepository<Employee> employeeRepo,
    IUnitOfWork unitOfWork)
    : INotificationHandler<OnboardingInstanceCompletedDomainEvent>
{
    public async Task Handle(OnboardingInstanceCompletedDomainEvent notification, CancellationToken ct)
    {
        var employee = await employeeRepo.GetFirstByConditionAsync(
            x => x.Id == notification.EmployeeId, null, ct);
        if (employee == null) return;

        employee.Activate("onboarding_completion");
        await unitOfWork.SaveChangesAsync(ct);
    }
}
