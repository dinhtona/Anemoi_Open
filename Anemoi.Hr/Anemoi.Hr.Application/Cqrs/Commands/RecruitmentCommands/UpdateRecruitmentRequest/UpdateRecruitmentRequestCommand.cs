using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRecruitmentRequest;

public sealed record UpdateRecruitmentRequestCommand(
    string Id,
    string DepartmentId,
    string PositionId,
    int RequestedHeadcount,
    string Reason,
    string PriorityCode,
    string UpdatedBy) : ICommandResult<RecruitmentRequestResponse>;
