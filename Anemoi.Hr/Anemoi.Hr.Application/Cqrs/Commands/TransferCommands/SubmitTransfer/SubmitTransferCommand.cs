using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.TransferCommands.SubmitTransfer;

public sealed record SubmitTransferCommand(
    EmployeeId EmployeeId,
    DepartmentId ToDepartmentId,
    PositionId ToPositionId,
    EmployeeId? ToManagerId,
    string ToGradeCode,
    DateOnly EffectiveDate,
    string Reason) : ICommandResult<EmployeeTransferDto>;
