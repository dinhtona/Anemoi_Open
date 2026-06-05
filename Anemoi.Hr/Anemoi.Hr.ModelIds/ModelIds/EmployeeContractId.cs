using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeContractId(Guid Value) : StronglyTypedId<Guid>(Value);
