using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record PayslipDocumentId(Guid Value) : StronglyTypedId<Guid>(Value);
