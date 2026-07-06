using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Identity.ModelIds;

namespace Anemoi.Centralize.Application.Cqrs.Requests.Identity;

public sealed record CreateSystemRoleGroupRequest(
    string Name,
    string Description,
    List<RoleId> IdentityRoleIds) : ICommandVoid;
