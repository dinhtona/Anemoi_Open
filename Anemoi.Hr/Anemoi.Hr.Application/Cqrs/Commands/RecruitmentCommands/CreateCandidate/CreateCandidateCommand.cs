using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateCandidate;

public sealed record CreateCandidateCommand(
    string CandidateCode,
    string FullName,
    string Email,
    string PhoneNumber,
    DateOnly? DateOfBirth,
    string Address,
    string ResumeUrl,
    string Source,
    string Notes,

    [property: JsonIgnore] string? CreatedBy = null) : ICommandResult<CandidateResponse>
{
}
