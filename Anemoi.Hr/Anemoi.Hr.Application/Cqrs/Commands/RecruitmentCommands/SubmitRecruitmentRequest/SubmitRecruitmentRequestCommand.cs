using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRecruitmentRequest;

public sealed record SubmitRecruitmentRequestCommand(
    string Id,
    string SubmittedBy) : ICommandResult<RecruitmentRequestResponse>;
