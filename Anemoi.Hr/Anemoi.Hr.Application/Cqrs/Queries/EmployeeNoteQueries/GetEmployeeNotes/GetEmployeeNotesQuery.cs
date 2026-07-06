using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeNoteQueries.GetEmployeeNotes;

public sealed record GetEmployeeNotesQuery(
    EmployeeId EmployeeId) : IQuery<IReadOnlyCollection<EmployeeNoteResponse>>;
