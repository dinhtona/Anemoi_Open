using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.FinalizePayrollRun;

public sealed record FinalizePayrollRunCommand(
    PayrollRunId PayrollRunId,
    [property: JsonIgnore] string FinalizedBy = null) : ICommandResult<PayrollRunDetailResponse>;
