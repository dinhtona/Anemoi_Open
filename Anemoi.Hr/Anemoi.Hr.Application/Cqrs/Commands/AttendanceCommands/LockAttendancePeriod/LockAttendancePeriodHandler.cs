using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.LockAttendancePeriod;

public sealed class LockAttendancePeriodHandler(
    ISqlRepository<AttendancePeriod> attendancePeriodRepository,
    IUnitOfWork unitOfWork,
    AttendanceMapper mapper)
    : ICommandHandler<LockAttendancePeriodCommand, OneOf<AttendancePeriodResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AttendancePeriodResponse, ErrorDetailResponse>> Handle(
        LockAttendancePeriodCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch period
        var period = await attendancePeriodRepository.GetFirstByConditionAsync(
            x => x.Id == request.AttendancePeriodId,
            null,
            cancellationToken);

        if (period is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodNotFound);

        // 2. Reject if already locked
        if (period.StatusCode == "Locked")
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodLocked);

        // 3. Update status
        period.StatusCode = "Locked";
        period.UpdatedAt = DateTime.UtcNow;
        period.UpdatedBy = request.UpdatedBy ?? "system";

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceConcurrencyConflict)
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return mapper.ToResponse(period);
    }
}
