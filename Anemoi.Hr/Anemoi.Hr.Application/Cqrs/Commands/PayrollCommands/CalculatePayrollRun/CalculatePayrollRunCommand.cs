using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CalculatePayrollRun;

public sealed record CalculatePayrollRunCommand(
    PayrollPeriodId PayrollPeriodId,
    EmployeeId EmployeeId,
    decimal PaidWorkingDays,
    decimal UnpaidLeaveDays,
    [property: JsonIgnore] string CalculatedBy = null) : ICommandResult<PayrollRunDetailResponse>;
