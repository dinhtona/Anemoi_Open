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
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CreatePayrollPeriod;

public sealed class CreatePayrollPeriodHandler(
    ISqlRepository<PayrollPeriod> payrollPeriodRepository,
    IUnitOfWork unitOfWork,
    PayrollMapper mapper)
    : ICommandHandler<CreatePayrollPeriodCommand, OneOf<PayrollPeriodResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayrollPeriodResponse, ErrorDetailResponse>> Handle(
        CreatePayrollPeriodCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check duplicate PeriodCode
        var isDuplicate = await payrollPeriodRepository.ExistByConditionAsync(
            x => x.PeriodCode == request.PeriodCode,
            cancellationToken);

        if (isDuplicate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollPeriodCodeAlreadyExists);

        // 2. Create entity
        var period = new PayrollPeriod
        {
            Id = new PayrollPeriodId(IdGenerator.NextGuid()),
            PeriodCode = request.PeriodCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            StandardWorkingDays = request.StandardWorkingDays,
            AttendancePeriodId = request.AttendancePeriodId.HasValue ? new AttendancePeriodId(request.AttendancePeriodId.Value) : null,
            StatusCode = PayrollPeriodStatusCode.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy ?? PayrollConstants.SystemActor,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = request.CreatedBy ?? PayrollConstants.SystemActor
        };

        await payrollPeriodRepository.CreateOneAsync(period, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.PayrollPeriodConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return mapper.ToResponse(period);
    }
}
