using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.UpdateEmployeeAllowance;

public record UpdateEmployeeAllowanceCommand(
    EmployeeAllowanceId EmployeeAllowanceId,
    decimal Amount,
    string Currency,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool SensitivePermissionConfirmed,
    [property: JsonIgnore] string UpdatedBy = null) : ICommandResult<UpdateEmployeeAllowanceResponse>;
