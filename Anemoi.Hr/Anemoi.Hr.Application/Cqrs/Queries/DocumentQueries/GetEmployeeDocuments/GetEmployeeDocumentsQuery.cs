using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.DocumentQueries.GetEmployeeDocuments;

public sealed record GetEmployeeDocumentsQuery(
    EmployeeId EmployeeId) : IQuery<IReadOnlyCollection<EmployeeDocumentResponse>>;
