using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CreatePayrollPeriod;

public sealed record CreatePayrollPeriodCommand(
    string PeriodCode,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal StandardWorkingDays,
    Guid? AttendancePeriodId = null,
    [property: JsonIgnore] string CreatedBy = null) : ICommandResult<PayrollPeriodResponse>;
