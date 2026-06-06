using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RejectPayrollRun;

public sealed record RejectPayrollRunCommand(
    PayrollRunId PayrollRunId,
    string RejectionReason,
    [property: JsonIgnore] string RejectedBy = null) : ICommandResult<PayrollRunDetailResponse>;
