using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeNoteQueries.GetEmployeeNote;

public sealed record GetEmployeeNoteQuery(
    EmployeeNoteId Id) : IQueryOne<EmployeeNoteResponse>;
