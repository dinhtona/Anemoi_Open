using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.TransferEmployee;

public sealed class TransferEmployeeHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<EmployeeDepartmentHistory> employeeDepartmentHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TransferEmployeeCommand, OneOf<EmployeeDepartmentHistoryIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeDepartmentHistoryIdResponse, ErrorDetailResponse>> Handle(TransferEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId,
            null,
            cancellationToken);
        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var targetDept = await departmentRepository.GetFirstByConditionAsync(
            x => x.Id == request.NewDepartmentId,
            null,
            cancellationToken);
        if (targetDept is null || !targetDept.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentNotFound);

        if (request.EffectiveDate < employee.JoinDate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentTransferDateBeforeJoinDate);

        var histories = await employeeDepartmentHistoryRepository.GetManyByConditionAsync(
            x => x.EmployeeId == request.EmployeeId,
            null,
            cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oldDeptId = employee.PrimaryDepartmentId;

        if (histories.Count > 0)
        {
            var latestHistory = histories.OrderByDescending(x => x.EffectiveFrom).First();

            if (request.EffectiveDate <= latestHistory.EffectiveFrom)
                return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentTransferOverlapping);

            if (latestHistory.DepartmentId == request.NewDepartmentId)
                return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentTransferSameDepartment);

            // Close the previous active record
            latestHistory.EffectiveTo = request.EffectiveDate.AddDays(-1);
            oldDeptId = latestHistory.DepartmentId;
        }
        else
        {
            // Self-healing: backfill initial history if somehow none exists
            if (employee.PrimaryDepartmentId == request.NewDepartmentId)
                return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentTransferSameDepartment);

            var initialHistory = new EmployeeDepartmentHistory
            {
                Id = new EmployeeDepartmentHistoryId(IdGenerator.NextGuid()),
                EmployeeId = request.EmployeeId,
                DepartmentId = employee.PrimaryDepartmentId,
                OldDepartmentId = null,
                IsPrimary = true,
                EffectiveFrom = employee.JoinDate,
                EffectiveTo = request.EffectiveDate.AddDays(-1),
                ReasonCode = "initial_placement",
                CreatedBy = "system:lazy_init",
                CreatedAt = DateTime.UtcNow
            };
            await employeeDepartmentHistoryRepository.CreateOneAsync(initialHistory, cancellationToken);
        }

        // Create new history entry
        var newHistory = new EmployeeDepartmentHistory
        {
            Id = new EmployeeDepartmentHistoryId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            DepartmentId = request.NewDepartmentId,
            OldDepartmentId = oldDeptId,
            IsPrimary = true,
            EffectiveFrom = request.EffectiveDate,
            EffectiveTo = null,
            ReasonCode = request.ReasonCode,
            CreatedBy = request.CreatedBy ?? PayrollConstants.SystemActor,
            CreatedAt = DateTime.UtcNow
        };
        await employeeDepartmentHistoryRepository.CreateOneAsync(newHistory, cancellationToken);

        // Update active department if immediate/past date
        if (request.EffectiveDate <= today)
        {
            employee.PrimaryDepartmentId = request.NewDepartmentId;
            employee.UpdatedAt = DateTime.UtcNow;
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return new EmployeeDepartmentHistoryIdResponse(newHistory.Id.Value.ToString());
    }
}
