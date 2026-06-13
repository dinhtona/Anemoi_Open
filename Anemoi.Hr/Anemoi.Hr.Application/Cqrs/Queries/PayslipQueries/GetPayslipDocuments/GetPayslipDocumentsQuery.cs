using Anemoi.Hr.Application.Responses;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocuments;

public sealed record GetPayslipDocumentsQuery(Guid PayslipId)
    : IQueryOne<IReadOnlyCollection<PayslipDocumentResponse>>;
