using System;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRecruitmentRequest;

[Obsolete("Use RejectWorkflowStepCommand instead")]
public sealed record RejectRecruitmentRequestCommand(
    string Id,
    string RejectedBy,
    string Comment) : ICommandResult<RecruitmentRequestResponse>;
