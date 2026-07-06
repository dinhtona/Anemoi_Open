using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.FinalizePayrollRun;

public sealed class FinalizePayrollRunHandler(
    ISqlRepository<PayrollRun> payrollRunRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    PayrollMapper mapper)
    : ICommandHandler<FinalizePayrollRunCommand, OneOf<PayrollRunDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayrollRunDetailResponse, ErrorDetailResponse>> Handle(
        FinalizePayrollRunCommand request,
        CancellationToken cancellationToken)
    {
        var run = await payrollRunRepository.GetFirstByConditionAsync(
            x => x.Id == request.PayrollRunId,
            q => q.Include(r => r.PayrollItems),
            cancellationToken);

        if (run is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunNotFound);

        if (!run.FinalizeRun(request.FinalizedBy ?? PayrollConstants.SystemActor, DateTime.UtcNow))
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunInvalidStatus);

        await publishEndpoint.Publish(new PayrollRunFinalizedIntegrationEvent(
            run.Id.Value.ToString(),
            request.FinalizedBy ?? PayrollConstants.SystemActor), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToDetailResponse(run);
    }
}
