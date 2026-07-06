using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record PublicHolidayId(Guid Value) : StronglyTypedId<Guid>(Value);

public sealed record CompanyHolidayId(Guid Value) : StronglyTypedId<Guid>(Value);

public sealed record WorkingCalendarRuleId(Guid Value) : StronglyTypedId<Guid>(Value);

public sealed record CalendarExceptionId(Guid Value) : StronglyTypedId<Guid>(Value);
