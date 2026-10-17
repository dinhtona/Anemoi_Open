using Anemoi.Contract.Identity.Responses;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Requests.Identity;

/// <summary>
/// Local (Centralize-owned) request: fetches identity roles from the Identity
/// service and enriches them with localized permission group/description
/// metadata. Localization stays in Centralize so the HTTP request culture
/// (Accept-Language) is honored.
/// </summary>
public sealed record GetIdentityPolicyRolesRequest : IRequest<List<UserRoleResponse>>;
