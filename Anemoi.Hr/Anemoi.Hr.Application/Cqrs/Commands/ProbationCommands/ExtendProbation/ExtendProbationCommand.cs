using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.ExtendProbation;

public sealed record ExtendProbationCommand(
    ProbationRecordId Id,
    DateOnly NewEndDate) : ICommandResult<ProbationRecordDto>;
