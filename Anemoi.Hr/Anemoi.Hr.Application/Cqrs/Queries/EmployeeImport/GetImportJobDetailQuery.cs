using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;

public sealed record GetImportJobDetailQuery(BulkImportJobId JobId) : IQueryOne<EmployeeImportJobDetailResponse>;
