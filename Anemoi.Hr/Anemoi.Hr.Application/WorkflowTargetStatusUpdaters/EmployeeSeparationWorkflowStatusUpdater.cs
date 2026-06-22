using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Separations;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class EmployeeSeparationWorkflowStatusUpdater(
    ISqlRepository<EmployeeSeparation> separationRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeOrganizationHistory> organizationHistoryRepository,
    IUnitOfWork unitOfWork)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.EmployeeSeparation;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new EmployeeSeparationId(Guid.Parse(entityId));
        var separation = await separationRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (separation is null) return;

        separation.Approve(performedBy);

        var employee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == separation.EmployeeId, ct);
        if (employee is not null)
        {
            if (separation.SeparationTypeCode == SeparationTypeCode.Resignation)
                employee.Resign(performedBy);
            else
                employee.Terminate(performedBy);
        }

        var currentOrgHistory = await organizationHistoryRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.EmployeeId == separation.EmployeeId && x.EndDate == null, ct);
        if (currentOrgHistory is not null)
        {
            currentOrgHistory.EndDate = separation.LastWorkingDate ?? separation.SeparationDate;
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new EmployeeSeparationId(Guid.Parse(entityId));
        var separation = await separationRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (separation is null) return;

        separation.Reject(performedBy, reason ?? "Rejected by approver");
        await unitOfWork.SaveChangesAsync(ct);
    }
}
