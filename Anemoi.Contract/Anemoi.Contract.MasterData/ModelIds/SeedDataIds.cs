using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Contract.MasterData.ModelIds;

public record SeedServerId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public override string ToString() => base.ToString();
}

public record SeedFunctionId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public override string ToString() => base.ToString();
}

public record SeedTemplateId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public override string ToString() => base.ToString();
}
