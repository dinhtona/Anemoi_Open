using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RecalculatePayrollRun;

public sealed record RecalculatePayrollRunCommand(
    PayrollRunId PayrollRunId,
    decimal PaidWorkingDays,
    decimal UnpaidLeaveDays,
    bool SensitivePermissionConfirmed,
    [property: JsonIgnore] string CalculatedBy = null) : ICommandResult<PayrollRunDetailResponse>;
