using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeDocuments;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.DocumentQueries.GetEmployeeDocuments;

public sealed class GetEmployeeDocumentsHandler(
    ISqlRepository<EmployeeDocument> documentRepository,
    EmployeeDocumentMapper mapper)
    : IQueryHandler<GetEmployeeDocumentsQuery, IReadOnlyCollection<EmployeeDocumentResponse>>
{
    public async Task<IReadOnlyCollection<EmployeeDocumentResponse>> Handle(
        GetEmployeeDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await documentRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId && !x.IsArchived)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.ToResponses(documents);
    }
}
