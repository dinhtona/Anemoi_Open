using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeNotes;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeNoteQueries.GetEmployeeNotes;

public sealed class GetEmployeeNotesHandler(
    ISqlRepository<EmployeeNote> noteRepository,
    EmployeeNoteMapper mapper)
    : IQueryHandler<GetEmployeeNotesQuery, IReadOnlyCollection<EmployeeNoteResponse>>
{
    public async Task<IReadOnlyCollection<EmployeeNoteResponse>> Handle(
        GetEmployeeNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await noteRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId && !x.IsArchived)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.ToResponses(notes);
    }
}
