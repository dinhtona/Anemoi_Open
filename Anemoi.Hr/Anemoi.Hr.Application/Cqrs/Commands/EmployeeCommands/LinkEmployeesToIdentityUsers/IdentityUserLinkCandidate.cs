namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.LinkEmployeesToIdentityUsers;

public sealed record IdentityUserLinkCandidate(Guid IdentityUserId, string Email);
