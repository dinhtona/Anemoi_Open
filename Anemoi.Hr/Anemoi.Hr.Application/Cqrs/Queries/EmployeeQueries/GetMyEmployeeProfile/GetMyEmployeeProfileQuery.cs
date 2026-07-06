using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetMyEmployeeProfile;

public sealed record GetMyEmployeeProfileQuery(string UserId, string Email) : IQueryOne<EmployeeResponse>;
