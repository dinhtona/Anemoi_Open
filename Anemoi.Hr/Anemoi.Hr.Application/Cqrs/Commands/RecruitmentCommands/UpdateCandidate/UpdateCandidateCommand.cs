using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateCandidate;

public sealed record UpdateCandidateCommand(
    CandidateId Id,
    string FullName,
    string Email,
    string PhoneNumber,
    DateOnly? DateOfBirth,
    string Address,
    string ResumeUrl,
    string Notes,

    [property: JsonIgnore] string? UpdatedBy = null) : ICommandResult<CandidateResponse>
{
}
