using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Contract.Notification.ModelIds;

public sealed record NotificationActionAuditId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public override string ToString() => base.ToString();
}
