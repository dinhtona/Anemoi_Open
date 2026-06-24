using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyApproverPreview;

public sealed record GetMyApproverPreviewQuery(
    string UserId,
    string Email) : IQueryOne<EssApproverPreviewResponse>;
