using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.UpdateAttendanceRecord;

public sealed class UpdateAttendanceRecordHandler(
    ISqlRepository<AttendancePeriod> attendancePeriodRepository,
    ISqlRepository<AttendanceRecord> attendanceRecordRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    AttendanceMapper mapper)
    : ICommandHandler<UpdateAttendanceRecordCommand, OneOf<AttendanceRecordResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AttendanceRecordResponse, ErrorDetailResponse>> Handle(
        UpdateAttendanceRecordCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch and validate existing record
        var record = await attendanceRecordRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        if (record is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceRecordNotFound);

        // 2. Fetch and validate period
        var period = await attendancePeriodRepository.GetFirstByConditionAsync(
            x => x.Id == request.AttendancePeriodId,
            null,
            cancellationToken);

        if (period is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodNotFound);

        if (period.StatusCode == AttendancePeriodStatusCode.Locked)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodLocked);

        // Check original period as well in case it's different and locked
        if (record.AttendancePeriodId != request.AttendancePeriodId)
        {
            var originalPeriod = await attendancePeriodRepository.GetFirstByConditionAsync(
                x => x.Id == record.AttendancePeriodId,
                null,
                cancellationToken);

            if (originalPeriod?.StatusCode == AttendancePeriodStatusCode.Locked)
                return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodLocked);
        }

        // Validate work date is within the period
        if (request.WorkDate < period.StartDate || request.WorkDate > period.EndDate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ValWorkDateOutOfPeriod);

        // 3. Validate employee
        var employeeExists = await employeeRepository.ExistByConditionAsync(
            x => x.Id == request.EmployeeId,
            cancellationToken);

        if (!employeeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        // 4. Check duplicate record for same employee and date (excluding current record)
        var isDuplicate = await attendanceRecordRepository.ExistByConditionAsync(
            x => x.EmployeeId == request.EmployeeId && x.WorkDate == request.WorkDate && x.Id != request.Id,
            cancellationToken);

        if (isDuplicate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceRecordAlreadyExists);

        // 5. Update properties
        record.AttendancePeriodId = request.AttendancePeriodId;
        record.EmployeeId = request.EmployeeId;
        record.WorkDate = request.WorkDate;
        record.CheckInTime = request.CheckInTime;
        record.CheckOutTime = request.CheckOutTime;
        record.WorkedHours = request.WorkedHours;
        record.WorkedDays = request.WorkedDays;
        record.Status = request.StatusCode;
        record.LeaveRequestId = request.LeaveRequestId;
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = request.UpdatedBy ?? PayrollConstants.SystemActor;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return mapper.ToResponse(record);
    }
}
