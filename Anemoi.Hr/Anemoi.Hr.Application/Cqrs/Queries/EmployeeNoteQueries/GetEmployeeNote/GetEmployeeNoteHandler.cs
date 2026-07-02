using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeNotes;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeNoteQueries.GetEmployeeNote;

public sealed class GetEmployeeNoteHandler(
    ISqlRepository<EmployeeNote> noteRepository,
    EmployeeNoteMapper mapper)
    : IQueryHandler<GetEmployeeNoteQuery, OneOf<EmployeeNoteResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeNoteResponse, ErrorDetailResponse>> Handle(
        GetEmployeeNoteQuery request, CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);

        return note is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNoteNotFound)
            : mapper.ToResponse(note);
    }
}
