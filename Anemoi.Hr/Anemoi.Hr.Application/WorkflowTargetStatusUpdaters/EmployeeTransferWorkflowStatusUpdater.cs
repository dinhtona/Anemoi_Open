using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Transfers;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class EmployeeTransferWorkflowStatusUpdater(
    ISqlRepository<EmployeeTransfer> transferRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeOrganizationHistory> organizationHistoryRepository,
    IUnitOfWork unitOfWork)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.EmployeeTransfer;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new EmployeeTransferId(Guid.Parse(entityId));
        var transfer = await transferRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (transfer is null) return;

        transfer.Approve(performedBy);

        var employee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == transfer.EmployeeId, ct);
        if (employee is not null)
        {
            employee.PrimaryDepartmentId = transfer.TargetDepartmentId;
            employee.PrimaryPositionId = transfer.TargetPositionId;
            employee.UpdatedAt = DateTime.UtcNow;
        }

        var currentOrgHistory = await organizationHistoryRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.EmployeeId == transfer.EmployeeId && x.EndDate == null, ct);
        if (currentOrgHistory is not null)
        {
            currentOrgHistory.EndDate = transfer.EffectiveDate;
        }

        await organizationHistoryRepository.CreateOneAsync(new EmployeeOrganizationHistory
        {
            Id = new EmployeeOrganizationHistoryId(IdGenerator.NextGuid()),
            EmployeeId = transfer.EmployeeId,
            DepartmentId = transfer.TargetDepartmentId,
            PositionId = transfer.TargetPositionId,
            GradeCode = employee?.GradeCode ?? string.Empty,
            EffectiveDate = transfer.EffectiveDate,
            ChangeReasonCode = "Transfer"
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new EmployeeTransferId(Guid.Parse(entityId));
        var transfer = await transferRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (transfer is null) return;

        transfer.Reject(performedBy, reason ?? "Rejected by approver");
        await unitOfWork.SaveChangesAsync(ct);
    }
}
