using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.LockPayrollPeriod;

public sealed class LockPayrollPeriodHandler(
    ISqlRepository<PayrollPeriod> payrollPeriodRepository,
    ISqlRepository<AttendancePeriod> attendancePeriodRepository,
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

        // 2. Reject if already locked
        if (period.StatusCode == PayrollPeriodStatusCode.Locked)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollPeriodLocked);

        // 3. Validate AttendancePeriodId
        if (period.AttendancePeriodId is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollPeriodAttendancePeriodRequired);

        // 4. Fetch AttendancePeriod
        var attendancePeriod = await attendancePeriodRepository.GetFirstByConditionAsync(
            x => x.Id == period.AttendancePeriodId,
            null,
            cancellationToken);

        if (attendancePeriod is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodNotFound);

        if (attendancePeriod.StatusCode != AttendancePeriodStatusCode.Locked)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodNotLocked);

        var now = DateTime.UtcNow;
        var updatedBy = request.UpdatedBy ?? PayrollConstants.SystemActor;
        period.StatusCode = PayrollPeriodStatusCode.Locked;
        period.UpdatedAt = now;
        period.UpdatedBy = updatedBy;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.PayrollPeriodConcurrencyConflict);

        return mapper.ToResponse(period);
    }
}
