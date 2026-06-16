using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MoveCandidateApplicationToOffer;

public sealed record MoveCandidateApplicationToOfferCommand(CandidateApplicationId Id, string Note)
    : ICommandResult<CandidateApplicationResponse>
{
    [JsonIgnore]
    public string ChangedBy { get; set; }
}
