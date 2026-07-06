using Anemoi.Hr.Application.Responses;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmailsForPayrollRun;

public sealed record SendPayslipEmailsForPayrollRunCommand(
    Guid PayrollRunId,
    string? SentBy = null) : ICommandResult<PayslipBatchResultResponse>;
