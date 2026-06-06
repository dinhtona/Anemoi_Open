using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.LockPayrollPeriod;

public sealed record LockPayrollPeriodCommand(
    PayrollPeriodId PayrollPeriodId,
    [property: JsonIgnore] string UpdatedBy = null) : ICommandResult<PayrollPeriodResponse>;
