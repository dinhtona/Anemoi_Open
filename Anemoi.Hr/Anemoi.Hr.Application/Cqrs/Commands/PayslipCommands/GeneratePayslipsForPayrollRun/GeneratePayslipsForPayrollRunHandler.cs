using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipsForPayrollRun;

public sealed class GeneratePayslipsForPayrollRunHandler(
    ISqlRepository<PayrollRun> payrollRunRepository,
    ISqlRepository<PayrollPeriod> payrollPeriodRepository,
    ISqlRepository<Payslip> payslipRepository,
    IUnitOfWork unitOfWork,
    PayslipMapper mapper)
    : ICommandHandler<GeneratePayslipsForPayrollRunCommand, OneOf<IReadOnlyCollection<PayslipResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<PayslipResponse>, ErrorDetailResponse>> Handle(
        GeneratePayslipsForPayrollRunCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load PayrollRun
        var run = await payrollRunRepository.GetFirstByConditionAsync(
            x => x.Id == request.PayrollRunId,
            null,
            cancellationToken);

        if (run is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunNotFound);

        // 2. Validate run status is Finalized
        if (run.Status != PayrollRunStatus.Finalized)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunNotFinalized);

        // 3. Load PayrollPeriod for PeriodCode snapshot
        var period = await payrollPeriodRepository.GetFirstByConditionAsync(
            x => x.Id == run.PayrollPeriodId,
            null,
            cancellationToken);

        var periodCode = period?.PeriodCode ?? "UNKNOWN";

        // 4. Idempotency Check: check if payslip already exists for PayrollRunId + EmployeeId
        var existingPayslip = await payslipRepository.GetFirstByConditionAsync(
            x => x.PayrollRunId == run.Id && x.EmployeeId == run.EmployeeId,
            null,
            cancellationToken);

        if (existingPayslip is not null)
        {
            return new List<PayslipResponse> { mapper.ToResponse(existingPayslip) };
        }

        // 5. Generate new Payslip
        var payslip = new Payslip
        {
            Id = new PayslipId(IdGenerator.NextGuid()),
            PayrollRunId = run.Id,
            EmployeeId = run.EmployeeId,
            PeriodCode = periodCode,
            EmployeeCode = run.EmployeeCode,
            EmployeeName = run.EmployeeName,
            BaseSalarySnapshot = run.BaseSalary,
            DailyRateSnapshot = run.DailyRate,
            PaidWorkingDays = run.PaidWorkingDays,
            PaidLeaveDays = 0,
            UnpaidLeaveDays = run.UnpaidLeaveDays,
            BasePayAmount = run.BasePayAmount,
            AllowanceTotal = run.TotalAllowanceAmount,
            DeductionTotal = run.TotalDeductionAmount,
            GrossPay = run.GrossAmount,
            NetPay = run.NetAmount,
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = request.GeneratedBy ?? PayrollConstants.SystemActor
        };

        var createResult = await payslipRepository.CreateOneAsync(payslip, cancellationToken);
        if (createResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new List<PayslipResponse> { mapper.ToResponse(payslip) };
    }
}
