using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentRequest;

public sealed record CreateRecruitmentRequestCommand(
    string DepartmentId,
    string PositionId,
    int RequestedHeadcount,
    string Reason,
    string PriorityCode,
    string CreatedBy) : ICommandResult<RecruitmentRequestResponse>;
