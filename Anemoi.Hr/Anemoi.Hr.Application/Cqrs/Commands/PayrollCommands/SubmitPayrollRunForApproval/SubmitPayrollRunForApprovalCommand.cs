using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.SubmitPayrollRunForApproval;

public sealed record SubmitPayrollRunForApprovalCommand(
    PayrollRunId PayrollRunId,
    [property: JsonIgnore] string SubmittedBy = null) : ICommandResult<PayrollRunDetailResponse>;
