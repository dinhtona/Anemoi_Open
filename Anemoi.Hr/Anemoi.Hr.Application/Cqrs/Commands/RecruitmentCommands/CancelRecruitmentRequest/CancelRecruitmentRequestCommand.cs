using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRecruitmentRequest;

public sealed record CancelRecruitmentRequestCommand(
    string Id,
    string CancelledBy) : ICommandResult<RecruitmentRequestResponse>;
