using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.PassProbation;

public sealed record PassProbationCommand(
    ProbationRecordId Id,
    string Result,
    string Comment) : ICommandResult<ProbationRecordDto>;
