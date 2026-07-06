using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.FailProbation;

public sealed record FailProbationCommand(
    ProbationRecordId Id,
    string Comment) : ICommandResult<ProbationRecordDto>;
