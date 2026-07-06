using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmail;
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

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmailsForPayrollRun;

public sealed class SendPayslipEmailsForPayrollRunHandler(
    ISqlRepository<PayrollRun> payrollRunRepository,
    ISqlRepository<Payslip> payslipRepository,
    ISqlRepository<PayslipDocument> payslipDocumentRepository,
    IMediator mediator)
    : ICommandHandler<SendPayslipEmailsForPayrollRunCommand, OneOf<PayslipBatchResultResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayslipBatchResultResponse, ErrorDetailResponse>> Handle(
        SendPayslipEmailsForPayrollRunCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get PayrollRun
        var run = await payrollRunRepository.GetFirstByConditionAsync(
            x => x.Id == new PayrollRunId(request.PayrollRunId),
            null,
            cancellationToken);

        if (run is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunNotFound);

        // 2. Validate run status is Finalized
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
            // A. Validate status (must be Published)
            if (payslip.Status != PayslipStatus.Published)
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

            // B. Validate active document exists
            var activeDoc = await payslipDocumentRepository.GetFirstByConditionAsync(
                x => x.PayslipId == payslip.Id && x.IsActive,
                null,
                cancellationToken);

            if (activeDoc is null)
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

            // C. Dispatch via SendPayslipEmailCommand
            var result = await mediator.Send(
                new SendPayslipEmailCommand(payslip.Id.Value, request.SentBy, null),
                cancellationToken);

            if (result.IsT0)
            {
                var delivery = result.AsT0;
                if (delivery.Status == PayslipEmailDeliveryStatus.Sent.ToString())
                {
                    succeededCount++;
                    items.Add(new PayslipBatchItemResult
                    {
                        PayslipId = payslip.Id.Value,
                        EmployeeId = payslip.EmployeeId.Value,
                        Status = "Succeeded"
                    });
                }
                else // Status is Failed
                {
                    failedCount++;
                    items.Add(new PayslipBatchItemResult
                    {
                        PayslipId = payslip.Id.Value,
                        EmployeeId = payslip.EmployeeId.Value,
                        Status = "Failed",
                        ErrorCode = delivery.ErrorMessage
                    });
                }
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
