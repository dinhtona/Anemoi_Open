using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeDocuments;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.DocumentQueries.GetEmployeeDocuments;

public sealed class GetEmployeeDocumentsHandler(
    ISqlRepository<EmployeeDocument> documentRepository,
    EmployeeDocumentMapper mapper)
    : IQueryHandler<GetEmployeeDocumentsQuery, OneOf<IReadOnlyCollection<EmployeeDocumentResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EmployeeDocumentResponse>, ErrorDetailResponse>> Handle(
        GetEmployeeDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await documentRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId && !x.IsArchived)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return OneOf<IReadOnlyCollection<EmployeeDocumentResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(documents));
    }
}
