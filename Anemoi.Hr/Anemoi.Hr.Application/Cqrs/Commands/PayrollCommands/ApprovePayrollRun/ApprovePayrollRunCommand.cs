using System;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.ApprovePayrollRun;

[Obsolete("Use ApproveWorkflowStepCommand instead")]
public sealed record ApprovePayrollRunCommand(
    PayrollRunId PayrollRunId,
    [property: JsonIgnore] string ApprovedBy = null) : ICommandResult<PayrollRunDetailResponse>;
