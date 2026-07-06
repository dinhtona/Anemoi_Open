using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateSalaryRange;

public sealed record CreateSalaryRangeCommand(
    SalaryGradeId SalaryGradeId,
    decimal MinSalary,
    decimal MaxSalary,
    string Currency,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive,
    bool SensitivePermissionConfirmed) : ICommandResult<CreateSalaryRangeResponse>;
