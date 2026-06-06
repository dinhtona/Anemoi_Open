using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeSalary;

public sealed record ChangeSalaryCommand(
    EmployeeId EmployeeId,
    decimal BaseSalary,
    SalaryType SalaryType,
    string Currency,
    DateOnly EffectiveFrom,
    CompensationChangeReason Reason,
    bool SensitivePermissionConfirmed,
    [property: JsonIgnore] string CreatedBy = null) : ICommandResult<ChangeSalaryResponse>;
