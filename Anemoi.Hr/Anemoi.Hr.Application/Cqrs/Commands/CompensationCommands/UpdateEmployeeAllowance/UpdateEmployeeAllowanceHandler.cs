using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.UpdateEmployeeAllowance;

public sealed class UpdateEmployeeAllowanceHandler(
    ISqlRepository<EmployeeAllowance> employeeAllowanceRepository,
    ISqlRepository<PayrollRun> payrollRunRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEmployeeAllowanceCommand, OneOf<UpdateEmployeeAllowanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<UpdateEmployeeAllowanceResponse, ErrorDetailResponse>> Handle(
        UpdateEmployeeAllowanceCommand request,
        CancellationToken cancellationToken)
    {
        var allowance = await employeeAllowanceRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeAllowanceId,
            null,
            cancellationToken);

        if (allowance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeAllowanceNotFound);

        if (request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceInvalidEffectiveRange);

        // Conservative check: reject update when any finalized/approved payroll period
        // overlaps the proposed new allowance effective date range for the same employee
        var proposedEffectiveFrom = request.EffectiveFrom;
        var proposedEffectiveTo = request.EffectiveTo;

        var hasPayrollOverlap = await payrollRunRepository.ExistByConditionAsync(
            x => x.EmployeeId == allowance.EmployeeId &&
                 (x.Status == PayrollRunStatus.Finalized || x.Status == PayrollRunStatus.Approved) &&
                 x.PayrollPeriod.StartDate <= (proposedEffectiveTo ?? DateOnly.MaxValue) &&
                 x.PayrollPeriod.EndDate >= proposedEffectiveFrom,
            cancellationToken);

        if (hasPayrollOverlap)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceAlreadyUsedInPayroll);

        // Fetch existing allowances for same employee + allowance type, excluding current
        var existingAllowances = await employeeAllowanceRepository.GetManyByConditionAsync(
            x => x.EmployeeId == allowance.EmployeeId &&
                 x.AllowanceTypeId == allowance.AllowanceTypeId &&
                 x.Id != request.EmployeeAllowanceId,
            null,
            cancellationToken);

        // Check for duplicate EffectiveFrom with another allowance of same employee + type
        if (existingAllowances.Any(x => x.EffectiveFrom == request.EffectiveFrom))
            return HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceTimelineDuplicateDate);

        // Check for date range overlap with another allowance of same employee + type
        var hasOverlap = existingAllowances.Any(x =>
            request.EffectiveFrom <= (x.EffectiveTo ?? DateOnly.MaxValue) &&
            (request.EffectiveTo ?? DateOnly.MaxValue) >= x.EffectiveFrom);

        if (hasOverlap)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceTimelineOverlap);

        allowance.UpdateDetails(
            request.Amount,
            request.Currency?.Trim(),
            request.EffectiveFrom,
            request.EffectiveTo,
            request.UpdatedBy);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException ||
                   CompensationPersistenceErrors.IsUniqueConstraintViolation(saveResult.AsT1)
                ? HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return new UpdateEmployeeAllowanceResponse { EmployeeAllowanceId = allowance.Id.Value.ToString() };
    }
}
