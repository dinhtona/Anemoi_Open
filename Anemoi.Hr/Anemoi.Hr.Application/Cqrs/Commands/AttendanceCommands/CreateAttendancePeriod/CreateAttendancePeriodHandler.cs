using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.CreateAttendancePeriod;

public sealed class CreateAttendancePeriodHandler(
    ISqlRepository<AttendancePeriod> attendancePeriodRepository,
    IUnitOfWork unitOfWork,
    AttendanceMapper mapper)
    : ICommandHandler<CreateAttendancePeriodCommand, OneOf<AttendancePeriodResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AttendancePeriodResponse, ErrorDetailResponse>> Handle(
        CreateAttendancePeriodCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check duplicate PeriodCode
        var isDuplicate = await attendancePeriodRepository.ExistByConditionAsync(
            x => x.PeriodCode == request.PeriodCode,
            cancellationToken);

        if (isDuplicate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodDuplicated);

        // 2. Create entity
        var period = new AttendancePeriod
        {
            Id = new AttendancePeriodId(IdGenerator.NextGuid()),
            PeriodCode = request.PeriodCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            StatusCode = AttendancePeriodStatusCode.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy ?? PayrollConstants.SystemActor,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = request.CreatedBy ?? PayrollConstants.SystemActor
        };

        await attendancePeriodRepository.CreateOneAsync(period, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return mapper.ToResponse(period);
    }
}
