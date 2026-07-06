using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetShiftTemplateById;

public sealed record GetShiftTemplateByIdQuery(
    ShiftTemplateId Id) : IQuery<ShiftTemplateResponse>;
