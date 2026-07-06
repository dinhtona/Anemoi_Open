using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.SalaryGradeCommands.UpdateSalaryGrade;

public sealed record UpdateSalaryGradeCommand(
    SalaryGradeId Id,
    string GradeCode,
    string Name,
    string Description) : ICommandResult<SalaryGradeResponse>;
