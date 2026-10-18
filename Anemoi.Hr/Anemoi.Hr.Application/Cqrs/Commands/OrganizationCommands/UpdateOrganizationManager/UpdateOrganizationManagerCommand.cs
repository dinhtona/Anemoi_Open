using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.OrganizationCommands.UpdateOrganizationManager;

public sealed record UpdateOrganizationManagerCommand(
    EmployeeId EmployeeId,
    EmployeeId? ManagerId) : ICommandResult<SuccessResponse>;
