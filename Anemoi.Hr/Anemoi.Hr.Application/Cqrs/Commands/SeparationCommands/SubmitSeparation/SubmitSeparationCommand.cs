using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.SeparationCommands.SubmitSeparation;

public sealed record SubmitSeparationCommand(
    EmployeeId EmployeeId,
    string SeparationType,
    string Reason,
    DateOnly LastWorkingDate) : ICommandResult<EmployeeSeparationDto>;
