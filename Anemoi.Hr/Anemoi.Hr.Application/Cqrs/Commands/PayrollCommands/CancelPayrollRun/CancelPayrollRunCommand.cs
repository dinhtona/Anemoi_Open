using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CancelPayrollRun;

public sealed record CancelPayrollRunCommand(
    PayrollRunId PayrollRunId,
    string CancellationReason = null,
    [property: JsonIgnore] string CancelledBy = null) : ICommandResult<PayrollRunDetailResponse>;
