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

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RejectPayrollRun;

public sealed class RejectPayrollRunHandler(
    ISqlRepository<PayrollRun> payrollRunRepository,
    IUnitOfWork unitOfWork,
    PayrollMapper mapper)
    : ICommandHandler<RejectPayrollRunCommand, OneOf<PayrollRunDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayrollRunDetailResponse, ErrorDetailResponse>> Handle(
        RejectPayrollRunCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RejectionReason))
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunRejectionReasonRequired);

        var run = await payrollRunRepository.GetFirstByConditionAsync(
            x => x.Id == request.PayrollRunId,
            q => q.Include(r => r.PayrollItems),
            cancellationToken);

        if (run is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunNotFound);

        if (!run.Reject(request.RejectedBy ?? "system", DateTime.UtcNow, request.RejectionReason))
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunInvalidStatus);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return mapper.ToDetailResponse(run);
    }
}
