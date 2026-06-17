using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentOpening;

public sealed record CreateRecruitmentOpeningCommand(
    string RecruitmentRequestId,
    string Code,
    int PlannedHeadcount,
    string CreatedBy) : ICommandResult<RecruitmentOpeningResponse>;
