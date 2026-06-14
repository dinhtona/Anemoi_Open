using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Application.Services;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.LockAttendancePeriod;

public sealed class LockAttendancePeriodHandler(
    ISqlRepository<AttendancePeriod> attendancePeriodRepository,
    ISqlRepository<AttendanceRecord> attendanceRecordRepository,
    ISqlRepository<AttendanceSummary> attendanceSummaryRepository,
    IUnitOfWork unitOfWork,
    AttendanceMapper mapper,
    ILogger<LockAttendancePeriodHandler> logger)
    : ICommandHandler<LockAttendancePeriodCommand, OneOf<AttendancePeriodResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AttendancePeriodResponse, ErrorDetailResponse>> Handle(
        LockAttendancePeriodCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Attendance period lock started. AttendancePeriodId: {AttendancePeriodId}",
            request.AttendancePeriodId.Value);

        var period = await attendancePeriodRepository.GetFirstByConditionAsync(
            x => x.Id == request.AttendancePeriodId,
            null,
            cancellationToken);

        if (period is null)
        {
            LogFailure(HrBusinessErrorCodes.AttendancePeriodNotFound, request.AttendancePeriodId);
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodNotFound);
        }

        if (period.StatusCode == "Locked")
        {
            LogFailure(HrBusinessErrorCodes.AttendancePeriodAlreadyLocked, request.AttendancePeriodId);
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodAlreadyLocked);
        }

        var records = await attendanceRecordRepository.GetManyByConditionAsync(
            x => x.AttendancePeriodId == request.AttendancePeriodId,
            null,
            cancellationToken);

        logger.LogInformation(
            "Attendance records loaded for lock. AttendancePeriodId: {AttendancePeriodId}, AttendanceRecordCount: {AttendanceRecordCount}",
            request.AttendancePeriodId.Value,
            records.Count);

        if (records.Count == 0)
        {
            LogFailure(HrBusinessErrorCodes.AttendanceRecordsNotFoundForPeriod, request.AttendancePeriodId);
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceRecordsNotFoundForPeriod);
        }

        var summariesExist = await attendanceSummaryRepository.ExistByConditionAsync(
            x => x.AttendancePeriodId == request.AttendancePeriodId,
            cancellationToken);

        if (summariesExist)
        {
            LogFailure(HrBusinessErrorCodes.AttendanceSummaryAlreadyExists, request.AttendancePeriodId);
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceSummaryAlreadyExists);
        }

        var employeeGroups = records.GroupBy(x => x.EmployeeId).ToList();
        logger.LogInformation(
            "Attendance records grouped for summary generation. AttendancePeriodId: {AttendancePeriodId}, EmployeeGroupCount: {EmployeeGroupCount}",
            request.AttendancePeriodId.Value,
            employeeGroups.Count);

        var now = DateTime.UtcNow;
        var updatedBy = request.UpdatedBy ?? "system";
        var summaries = new List<AttendanceSummary>(employeeGroups.Count);

        foreach (var group in employeeGroups)
        {
            var totals = AttendanceSummaryCalculator.Calculate(group);
            summaries.Add(new AttendanceSummary
            {
                Id = new AttendanceSummaryId(IdGenerator.NextGuid()),
                AttendancePeriodId = request.AttendancePeriodId,
                EmployeeId = group.Key,
                WorkedDays = totals.WorkedDays,
                WorkedHours = totals.WorkedHours,
                LeaveDays = totals.LeaveDays,
                AbsentDays = totals.AbsentDays,
                HolidayDays = totals.HolidayDays,
                PaidWorkingDays = totals.PaidWorkingDays,
                PaidLeaveDays = totals.PaidLeaveDays,
                UnpaidLeaveDays = totals.UnpaidLeaveDays,
                CreatedAt = now,
                CreatedBy = updatedBy,
                UpdatedAt = now,
                UpdatedBy = updatedBy
            });
        }

        var createResult = await attendanceSummaryRepository.CreateManyAsync(summaries, cancellationToken);
        if (createResult.IsT1)
        {
            LogFailure(HrBusinessErrorCodes.AttendanceSummaryGenerationFailed, request.AttendancePeriodId);
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceSummaryGenerationFailed);
        }

        var originalStatus = period.StatusCode;
        var originalUpdatedAt = period.UpdatedAt;
        var originalUpdatedBy = period.UpdatedBy;
        period.StatusCode = "Locked";
        period.UpdatedAt = now;
        period.UpdatedBy = updatedBy;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
        {
            period.StatusCode = originalStatus;
            period.UpdatedAt = originalUpdatedAt;
            period.UpdatedBy = originalUpdatedBy;

            var errorCode = saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrBusinessErrorCodes.AttendanceConcurrencyConflict
                : HrBusinessErrorCodes.AttendanceSummaryGenerationFailed;
            LogFailure(errorCode, request.AttendancePeriodId);
            return HrErrorResponses.Create(errorCode);
        }

        logger.LogInformation(
            "Attendance summaries created. AttendancePeriodId: {AttendancePeriodId}, AttendanceSummaryCount: {AttendanceSummaryCount}",
            request.AttendancePeriodId.Value,
            summaries.Count);
        logger.LogInformation(
            "Attendance period lock completed. AttendancePeriodId: {AttendancePeriodId}",
            request.AttendancePeriodId.Value);

        return mapper.ToResponse(period);
    }

    private void LogFailure(string failureCode, AttendancePeriodId attendancePeriodId)
    {
        logger.LogWarning(
            "Attendance period lock failed. AttendancePeriodId: {AttendancePeriodId}, FailureCode: {FailureCode}",
            attendancePeriodId.Value,
            failureCode);
    }
}
