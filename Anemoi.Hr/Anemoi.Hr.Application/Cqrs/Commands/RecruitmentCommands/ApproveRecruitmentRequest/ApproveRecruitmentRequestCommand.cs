using System;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRecruitmentRequest;

[Obsolete("Use ApproveWorkflowStepCommand instead")]
public sealed record ApproveRecruitmentRequestCommand(
    string Id,
    string ApprovedBy,
    string Comment) : ICommandResult<RecruitmentRequestResponse>;
