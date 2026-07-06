using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.StartProbation;

public sealed record StartProbationCommand(
    EmployeeId EmployeeId,
    DateOnly StartDate,
    DateOnly EndDate) : ICommandResult<ProbationRecordDto>;
