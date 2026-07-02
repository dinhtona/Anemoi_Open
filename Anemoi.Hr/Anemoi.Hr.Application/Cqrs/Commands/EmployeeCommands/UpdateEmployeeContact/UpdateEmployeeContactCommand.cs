using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.UpdateEmployeeContact;

public sealed record UpdateEmployeeContactCommand(
    EmployeeId EmployeeId,
    string? WorkEmail,
    string? PersonalEmail,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? AvatarStorageKey,
    [property: JsonIgnore] string CreatedBy = null) : ICommandVoid;
