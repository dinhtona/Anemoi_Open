using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.SubmitPayrollRunForApproval;

public sealed class SubmitPayrollRunForApprovalHandler(
    ISqlRepository<PayrollRun> payrollRunRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IWorkflowEngine workflowEngine,
    PayrollMapper mapper)
    : ICommandHandler<SubmitPayrollRunForApprovalCommand, OneOf<PayrollRunDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayrollRunDetailResponse, ErrorDetailResponse>> Handle(
        SubmitPayrollRunForApprovalCommand request,
        CancellationToken cancellationToken)
    {
        var run = await payrollRunRepository.GetFirstByConditionAsync(
            x => x.Id == request.PayrollRunId,
            q => q.Include(r => r.PayrollItems),
            cancellationToken);

        if (run is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunNotFound);

        if (!run.SubmitForApproval(request.SubmittedBy ?? PayrollConstants.SystemActor, DateTime.UtcNow))
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunInvalidStatus);

        if (request.SubmittedBy is not null)
        {
            var requesterUserId = new UserId(Guid.Parse(request.SubmittedBy));
            var workflowResult = await workflowEngine.StartAsync(
                WorkflowConstants.TargetEntityTypes.PayrollRun,
                run.Id.Value,
                run.EmployeeId,
                requesterUserId,
                requesterUserId,
                cancellationToken);

            if (workflowResult.TryPickT1(out var workflowError, out _))
                return workflowError;
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        await publishEndpoint.Publish(new PayrollRunSubmittedIntegrationEvent(
            run.Id.Value.ToString(),
            request.SubmittedBy ?? PayrollConstants.SystemActor), cancellationToken);

        return mapper.ToDetailResponse(run);
    }
}
