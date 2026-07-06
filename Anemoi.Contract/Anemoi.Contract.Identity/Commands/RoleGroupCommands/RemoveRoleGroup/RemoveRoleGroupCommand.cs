using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Identity.ModelIds;
using Newtonsoft.Json;

namespace Anemoi.Contract.Identity.Commands.RoleGroupCommands.RemoveRoleGroup;

public sealed record RemoveRoleGroupCommand(RoleGroupId Id,
    [property: JsonIgnore] bool RequireSystemWide = false,
    [property: JsonIgnore] string WorkspaceId = null) : ICommandVoid;
