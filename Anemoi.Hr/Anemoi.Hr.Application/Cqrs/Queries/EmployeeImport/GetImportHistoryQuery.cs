using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;

public sealed record GetImportHistoryQuery : GetManyQuery, IQueryOne<PaginationResponse<EmployeeImportHistoryResponse>>;
