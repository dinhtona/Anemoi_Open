using Anemoi.Hr.Application.Responses;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdfsForPayrollRun;

public sealed record GeneratePayslipPdfsForPayrollRunCommand(
    Guid PayrollRunId,
    string? GeneratedBy = null,
    bool ForceRegenerate = false) : ICommandResult<PayslipBatchResultResponse>;
