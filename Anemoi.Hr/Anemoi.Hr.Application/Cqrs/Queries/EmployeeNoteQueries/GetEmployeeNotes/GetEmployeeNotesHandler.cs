using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeNotes;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeNoteQueries.GetEmployeeNotes;

public sealed class GetEmployeeNotesHandler(
    ISqlRepository<EmployeeNote> noteRepository,
    EmployeeNoteMapper mapper)
    : IQueryHandler<GetEmployeeNotesQuery, OneOf<IReadOnlyCollection<EmployeeNoteResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EmployeeNoteResponse>, ErrorDetailResponse>> Handle(
        GetEmployeeNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await noteRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId && !x.IsArchived)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return OneOf<IReadOnlyCollection<EmployeeNoteResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(notes));
    }
}
