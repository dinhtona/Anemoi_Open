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

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.FinalizePayrollRun;

public sealed class FinalizePayrollRunHandler(
    ISqlRepository<PayrollRun> payrollRunRepository,
    IUnitOfWork unitOfWork,
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

        if (!run.FinalizeRun(request.FinalizedBy ?? "system", DateTime.UtcNow))
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunInvalidStatus);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return mapper.ToDetailResponse(run);
    }
}
