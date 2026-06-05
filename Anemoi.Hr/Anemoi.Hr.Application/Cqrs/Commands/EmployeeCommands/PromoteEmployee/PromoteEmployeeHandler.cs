using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.PromoteEmployee;

public sealed class PromoteEmployeeHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Position> positionRepository,
    ISqlRepository<EmployeePositionHistory> employeePositionHistoryRepository,
    ISqlRepository<EmployeeGradeHistory> employeeGradeHistoryRepository,
    IEmployeeGradeLookup employeeGradeLookup,
    HrSettings hrSettings,
    IUnitOfWork unitOfWork)
    : ICommandHandler<PromoteEmployeeCommand, OneOf<PromoteEmployeeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PromoteEmployeeResponse, ErrorDetailResponse>> Handle(
        PromoteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var today = GetBusinessToday();

        if (request.EffectiveDate > today)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PromotionEffectiveDateInFuture);

        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId,
            null,
            cancellationToken);
        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        if (request.EffectiveDate < employee.JoinDate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PromotionDateBeforeJoinDate);

        var changesPosition = request.NewPositionId is not null;
        var changesGrade = !string.IsNullOrWhiteSpace(request.NewGradeCode);

        if (!changesPosition && !changesGrade)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PromotionNoChange);

        EmployeePositionHistory newPositionHistory = null;
        EmployeeGradeHistory newGradeHistory = null;

        if (changesPosition)
        {
            if (employee.PrimaryPositionId == request.NewPositionId)
                return HrErrorResponses.Create(HrBusinessErrorCodes.PositionChangeSamePosition);

            var targetPosition = await positionRepository.GetFirstByConditionAsync(
                x => x.Id == request.NewPositionId,
                null,
                cancellationToken);
            if (targetPosition is null || !targetPosition.IsActive)
                return HrErrorResponses.Create(HrBusinessErrorCodes.PositionNotFound);

            var positionResult = await CreatePositionHistoryAsync(employee, request, cancellationToken);
            if (positionResult.IsT1)
                return positionResult.AsT1;

            newPositionHistory = positionResult.AsT0;
        }

        if (changesGrade)
        {
            var newGradeCode = request.NewGradeCode.Trim();
            if (!employeeGradeLookup.IsValidGrade(newGradeCode))
                return HrErrorResponses.Create(HrBusinessErrorCodes.GradeNotFound);

            if (string.Equals(employee.GradeCode, newGradeCode, StringComparison.OrdinalIgnoreCase))
                return HrErrorResponses.Create(HrBusinessErrorCodes.GradeChangeSameGrade);

            var gradeResult = await CreateGradeHistoryAsync(employee, request with { NewGradeCode = newGradeCode }, cancellationToken);
            if (gradeResult.IsT1)
                return gradeResult.AsT1;

            newGradeHistory = gradeResult.AsT0;
        }

        employee.PrimaryPositionId = changesPosition ? request.NewPositionId : employee.PrimaryPositionId;
        employee.GradeCode = changesGrade ? request.NewGradeCode.Trim() : employee.GradeCode;
        employee.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.PromotionConcurrencyConflict)
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return new PromoteEmployeeResponse
        {
            EmployeeId = employee.Id.Value.ToString(),
            PositionHistoryId = newPositionHistory?.Id.Value.ToString(),
            GradeHistoryId = newGradeHistory?.Id.Value.ToString()
        };
    }

    private async Task<OneOf<EmployeePositionHistory, ErrorDetailResponse>> CreatePositionHistoryAsync(
        Employee employee,
        PromoteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var histories = await employeePositionHistoryRepository.GetManyByConditionAsync(
            x => x.EmployeeId == request.EmployeeId,
            null,
            cancellationToken);

        var hasOverlap = histories.Any(h => h.EffectiveFrom >= request.EffectiveDate || (h.EffectiveTo != null && h.EffectiveTo >= request.EffectiveDate));
        if (hasOverlap)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PositionChangeOverlapping);

        var latestHistory = histories.OrderByDescending(x => x.EffectiveFrom).FirstOrDefault();
        if (latestHistory is not null && latestHistory.EffectiveTo == null)
        {
            latestHistory.EffectiveTo = request.EffectiveDate.AddDays(-1);
        }

        var oldPositionId = latestHistory?.PositionId ?? employee.PrimaryPositionId;
        var newHistory = new EmployeePositionHistory
        {
            Id = new EmployeePositionHistoryId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            PositionId = request.NewPositionId,
            OldPositionId = oldPositionId,
            EffectiveFrom = request.EffectiveDate,
            EffectiveTo = null,
            ReasonCode = request.ReasonCode,
            CreatedBy = request.CreatedBy ?? "system",
            CreatedAt = DateTime.UtcNow
        };

        await employeePositionHistoryRepository.CreateOneAsync(newHistory, cancellationToken);
        return newHistory;
    }

    private async Task<OneOf<EmployeeGradeHistory, ErrorDetailResponse>> CreateGradeHistoryAsync(
        Employee employee,
        PromoteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var histories = await employeeGradeHistoryRepository.GetManyByConditionAsync(
            x => x.EmployeeId == request.EmployeeId,
            null,
            cancellationToken);

        var hasOverlap = histories.Any(h => h.EffectiveFrom >= request.EffectiveDate || (h.EffectiveTo != null && h.EffectiveTo >= request.EffectiveDate));
        if (hasOverlap)
            return HrErrorResponses.Create(HrBusinessErrorCodes.GradeChangeOverlapping);

        var latestHistory = histories.OrderByDescending(x => x.EffectiveFrom).FirstOrDefault();
        if (latestHistory is not null && latestHistory.EffectiveTo == null)
        {
            latestHistory.EffectiveTo = request.EffectiveDate.AddDays(-1);
        }

        var oldGradeCode = latestHistory?.GradeCode ?? employee.GradeCode;
        var newHistory = new EmployeeGradeHistory
        {
            Id = new EmployeeGradeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            GradeCode = request.NewGradeCode,
            OldGradeCode = oldGradeCode,
            EffectiveFrom = request.EffectiveDate,
            EffectiveTo = null,
            ReasonCode = request.ReasonCode,
            CreatedBy = request.CreatedBy ?? "system",
            CreatedAt = DateTime.UtcNow
        };

        await employeeGradeHistoryRepository.CreateOneAsync(newHistory, cancellationToken);
        return newHistory;
    }

    private DateOnly GetBusinessToday()
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(hrSettings.BusinessTimeZone);
            var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            return DateOnly.FromDateTime(localTime.DateTime);
        }
        catch
        {
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }
}
