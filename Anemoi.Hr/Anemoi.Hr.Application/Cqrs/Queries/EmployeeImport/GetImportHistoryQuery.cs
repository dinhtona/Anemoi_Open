using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;

public sealed record GetImportHistoryQuery : GetManyQuery, IQueryPaged<EmployeeImportHistoryResponse>;
