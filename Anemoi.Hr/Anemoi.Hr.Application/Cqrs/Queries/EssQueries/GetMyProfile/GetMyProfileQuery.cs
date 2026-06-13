using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyProfile;

public sealed record GetMyProfileQuery(string UserId, string Email) : IQueryOne<EssEmployeeProfileResponse>;
