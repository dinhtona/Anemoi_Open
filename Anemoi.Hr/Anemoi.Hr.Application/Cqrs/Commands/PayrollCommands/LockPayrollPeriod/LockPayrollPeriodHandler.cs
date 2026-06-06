using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.LockPayrollPeriod;

public sealed class LockPayrollPeriodHandler(
    ISqlRepository<PayrollPeriod> payrollPeriodRepository,
    IUnitOfWork unitOfWork,
    PayrollMapper mapper)
    : ICommandHandler<LockPayrollPeriodCommand, OneOf<PayrollPeriodResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayrollPeriodResponse, ErrorDetailResponse>> Handle(
        LockPayrollPeriodCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch Payroll Period
        var period = await payrollPeriodRepository.GetFirstByConditionAsync(
            x => x.Id == request.PayrollPeriodId,
            null,
            cancellationToken);

        if (period is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollPeriodNotFound);

        // 2. Lock the period
        period.StatusCode = "Locked";
        period.UpdatedAt = DateTime.UtcNow;
        period.UpdatedBy = request.UpdatedBy ?? "system";

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create("HR_PAYROLL_PERIOD_CONCURRENCY_CONFLICT")
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return mapper.ToResponse(period);
    }
}
