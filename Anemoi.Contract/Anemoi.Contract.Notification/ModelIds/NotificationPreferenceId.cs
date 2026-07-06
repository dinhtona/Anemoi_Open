using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Contract.Notification.ModelIds;

public sealed record NotificationPreferenceId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public override string ToString() => base.ToString();
}
