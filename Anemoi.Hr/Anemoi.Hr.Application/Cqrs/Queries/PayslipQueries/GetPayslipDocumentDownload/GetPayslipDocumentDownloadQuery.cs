using Anemoi.Hr.Application.Responses;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using System;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload;

public sealed record GetPayslipDocumentDownloadQuery(Guid PayslipId, Guid DocumentId)
    : IQueryOne<PayslipDocumentDownloadResponse>;
