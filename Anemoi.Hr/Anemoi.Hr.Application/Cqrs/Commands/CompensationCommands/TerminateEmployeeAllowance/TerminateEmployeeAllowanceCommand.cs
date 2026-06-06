using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.TerminateEmployeeAllowance;

public record TerminateEmployeeAllowanceCommand(
    EmployeeAllowanceId EmployeeAllowanceId,
    DateOnly TerminationDate,
    bool SensitivePermissionConfirmed,
    [property: JsonIgnore] string UpdatedBy = null) : ICommandResult<TerminateEmployeeAllowanceResponse>;
