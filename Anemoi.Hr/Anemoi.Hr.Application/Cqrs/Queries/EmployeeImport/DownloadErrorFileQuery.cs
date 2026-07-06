using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;

public sealed record DownloadErrorFileQuery(BulkImportJobId JobId) : IQuery<OneOf<FileResponse, ErrorDetailResponse>>;
