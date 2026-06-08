using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record ReportExportAuditLogId(Guid Value) : StronglyTypedId<Guid>(Value);