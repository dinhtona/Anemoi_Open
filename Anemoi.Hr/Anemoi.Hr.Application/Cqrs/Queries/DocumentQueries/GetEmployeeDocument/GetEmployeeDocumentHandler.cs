using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeDocuments;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.DocumentQueries.GetEmployeeDocument;

public sealed class GetEmployeeDocumentHandler(
    ISqlRepository<EmployeeDocument> documentRepository,
    EmployeeDocumentMapper mapper)
    : IQueryHandler<GetEmployeeDocumentQuery, OneOf<EmployeeDocumentResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeDocumentResponse, ErrorDetailResponse>> Handle(
        GetEmployeeDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await documentRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);

        return document is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeDocumentNotFound)
            : mapper.ToResponse(document);
    }
}
