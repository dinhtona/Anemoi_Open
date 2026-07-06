using Anemoi.Hr.Application.Responses;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipEmailDeliveries;

public sealed record GetPayslipEmailDeliveriesQuery(Guid PayslipId)
    : IQueryOne<IReadOnlyCollection<PayslipEmailDeliveryResponse>>;
