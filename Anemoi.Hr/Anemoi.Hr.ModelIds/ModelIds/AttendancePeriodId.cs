using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record AttendancePeriodId(Guid Value) : StronglyTypedId<Guid>(Value);
