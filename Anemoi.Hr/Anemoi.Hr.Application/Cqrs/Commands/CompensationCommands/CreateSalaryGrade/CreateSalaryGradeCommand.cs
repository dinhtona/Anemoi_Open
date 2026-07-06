using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateSalaryGrade;

public sealed record CreateSalaryGradeCommand(
    string GradeCode,
    string Name,
    string Description,
    bool IsActive,
    bool SensitivePermissionConfirmed) : ICommandResult<CreateSalaryGradeResponse>;
