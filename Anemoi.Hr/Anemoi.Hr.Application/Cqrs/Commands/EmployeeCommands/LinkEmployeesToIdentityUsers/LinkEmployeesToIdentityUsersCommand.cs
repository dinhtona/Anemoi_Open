using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.LinkEmployeesToIdentityUsers;

public sealed record LinkEmployeesToIdentityUsersCommand(
    bool DryRun,
    IReadOnlyCollection<IdentityUserLinkCandidate> Candidates,
    [property: JsonIgnore] string CreatedBy = null) : ICommandResult<EmployeeIdentityLinkResultResponse>;
