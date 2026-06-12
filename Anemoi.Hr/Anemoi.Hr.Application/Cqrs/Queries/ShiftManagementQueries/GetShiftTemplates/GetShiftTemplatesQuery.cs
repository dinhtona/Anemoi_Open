using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetShiftTemplates;

public sealed record GetShiftTemplatesQuery(
    bool? IsActive = null,
    string Search = null,
    int Page = 1,
    int PageSize = 50,
    string SortBy = null,
    string SortDirection = "asc") : IQuery<PaginationResponse<ShiftTemplateResponse>>;
