using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using OneOf;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdfsForPayrollRun;

public sealed class GeneratePayslipPdfsForPayrollRunHandler(
    ISqlRepository<PayrollRun> payrollRunRepository,
    ISqlRepository<Payslip> payslipRepository,
    ISqlRepository<PayslipDocument> payslipDocumentRepository,
    IMediator mediator)
    : ICommandHandler<GeneratePayslipPdfsForPayrollRunCommand, OneOf<PayslipBatchResultResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayslipBatchResultResponse, ErrorDetailResponse>> Handle(
        GeneratePayslipPdfsForPayrollRunCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get PayrollRun
        var run = await payrollRunRepository.GetFirstByConditionAsync(
            x => x.Id == new PayrollRunId(request.PayrollRunId),
            null,
            cancellationToken);

        if (run is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunNotFound);

        // 2. Validate payroll run is Finalized
        if (run.Status != PayrollRunStatus.Finalized)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunNotFinalized);

        // 3. Load all payslips for this PayrollRun
        var payslips = await payslipRepository.GetManyByConditionAsync(
            x => x.PayrollRunId == run.Id,
            null,
            cancellationToken);

        int totalCount = payslips.Count;
        int succeededCount = 0;
        int skippedCount = 0;
        int failedCount = 0;
        var items = new List<PayslipBatchItemResult>();

        foreach (var payslip in payslips)
        {
            // Idempotency check for skip
            var activeDoc = await payslipDocumentRepository.GetFirstByConditionAsync(
                x => x.PayslipId == payslip.Id && x.IsActive,
                null,
                cancellationToken);

            if (activeDoc is not null && !request.ForceRegenerate)
            {
                skippedCount++;
                items.Add(new PayslipBatchItemResult
                {
                    PayslipId = payslip.Id.Value,
                    EmployeeId = payslip.EmployeeId.Value,
                    Status = "Skipped"
                });
                continue;
            }

            // Generate
            var result = await mediator.Send(
                new GeneratePayslipPdfCommand(payslip.Id.Value, request.GeneratedBy, request.ForceRegenerate),
                cancellationToken);

            if (result.IsT0)
            {
                succeededCount++;
                items.Add(new PayslipBatchItemResult
                {
                    PayslipId = payslip.Id.Value,
                    EmployeeId = payslip.EmployeeId.Value,
                    Status = "Succeeded"
                });
            }
            else
            {
                failedCount++;
                items.Add(new PayslipBatchItemResult
                {
                    PayslipId = payslip.Id.Value,
                    EmployeeId = payslip.EmployeeId.Value,
                    Status = "Failed",
                    ErrorCode = result.AsT1.Code
                });
            }
        }

        return new PayslipBatchResultResponse
        {
            Total = totalCount,
            Succeeded = succeededCount,
            Skipped = skippedCount,
            Failed = failedCount,
            Items = items
        };
    }
}
