using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
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

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.CreateAttendanceRecord;

public sealed class CreateAttendanceRecordHandler(
    ISqlRepository<AttendancePeriod> attendancePeriodRepository,
    ISqlRepository<AttendanceRecord> attendanceRecordRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    AttendanceMapper mapper)
    : ICommandHandler<CreateAttendanceRecordCommand, OneOf<AttendanceRecordResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AttendanceRecordResponse, ErrorDetailResponse>> Handle(
        CreateAttendanceRecordCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch and validate period
        var period = await attendancePeriodRepository.GetFirstByConditionAsync(
            x => x.Id == request.AttendancePeriodId,
            null,
            cancellationToken);

        if (period is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodNotFound);

        if (period.StatusCode == AttendancePeriodStatusCode.Locked)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodLocked);

        // Validate work date is within the period
        if (request.WorkDate < period.StartDate || request.WorkDate > period.EndDate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ValWorkDateOutOfPeriod);

        // 2. Validate employee
        var employeeExists = await employeeRepository.ExistByConditionAsync(
            x => x.Id == request.EmployeeId,
            cancellationToken);

        if (!employeeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        // 3. Check duplicate record for same employee and date
        var isDuplicate = await attendanceRecordRepository.ExistByConditionAsync(
            x => x.EmployeeId == request.EmployeeId && x.WorkDate == request.WorkDate,
            cancellationToken);

        if (isDuplicate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceRecordAlreadyExists);

        // 4. Create and save entity
        var record = new AttendanceRecord
        {
            Id = new AttendanceRecordId(IdGenerator.NextGuid()),
            AttendancePeriodId = request.AttendancePeriodId,
            EmployeeId = request.EmployeeId,
            WorkDate = request.WorkDate,
            CheckInTime = request.CheckInTime,
            CheckOutTime = request.CheckOutTime,
            WorkedHours = request.WorkedHours,
            WorkedDays = request.WorkedDays,
            Status = request.StatusCode,
            LeaveRequestId = request.LeaveRequestId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy ?? PayrollConstants.SystemActor,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = request.CreatedBy ?? PayrollConstants.SystemActor
        };

        await attendanceRecordRepository.CreateOneAsync(record, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.AttendanceConcurrencyConflict);

        return mapper.ToResponse(record);
    }
}
