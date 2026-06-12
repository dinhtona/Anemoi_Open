using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Anemoi.Hr.ModelIds.ModelIds;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class CalendarManagementMapper
{
    public PublicHolidayResponse ToPublicHolidayResponse(PublicHoliday source)
    {
        return new PublicHolidayResponse
        {
            Id = source.Id.Value,
            HolidayDate = source.HolidayDate,
            Name = source.Name,
            Description = source.Description,
            CountryCode = source.CountryCode,
            IsRecurringAnnual = source.IsRecurringAnnual,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public PublicHolidayIdResponse ToPublicHolidayIdResponse(PublicHoliday source)
    {
        return new PublicHolidayIdResponse
        {
            Id = source.Id.Value,
            CreatedAt = source.CreatedAt
        };
    }

    public CompanyHolidayResponse ToCompanyHolidayResponse(CompanyHoliday source)
    {
        return new CompanyHolidayResponse
        {
            Id = source.Id.Value,
            HolidayDate = source.HolidayDate,
            Name = source.Name,
            Description = source.Description,
            IsRecurringAnnual = source.IsRecurringAnnual,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public CompanyHolidayIdResponse ToCompanyHolidayIdResponse(CompanyHoliday source)
    {
        return new CompanyHolidayIdResponse
        {
            Id = source.Id.Value,
            CreatedAt = source.CreatedAt
        };
    }

    public WorkingCalendarRuleResponse ToWorkingCalendarRuleResponse(WorkingCalendarRule source)
    {
        return new WorkingCalendarRuleResponse
        {
            Id = source.Id.Value,
            Name = source.Name,
            Description = source.Description,
            WorkMonday = source.WorkMonday,
            WorkTuesday = source.WorkTuesday,
            WorkWednesday = source.WorkWednesday,
            WorkThursday = source.WorkThursday,
            WorkFriday = source.WorkFriday,
            WorkSaturday = source.WorkSaturday,
            WorkSunday = source.WorkSunday,
            EffectiveFrom = source.EffectiveFrom,
            EffectiveTo = source.EffectiveTo,
            IsActive = source.IsActive,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public WorkingCalendarRuleIdResponse ToWorkingCalendarRuleIdResponse(WorkingCalendarRule source)
    {
        return new WorkingCalendarRuleIdResponse
        {
            Id = source.Id.Value,
            CreatedAt = source.CreatedAt
        };
    }

    public CalendarExceptionResponse ToCalendarExceptionResponse(CalendarException source)
    {
        return new CalendarExceptionResponse
        {
            Id = source.Id.Value,
            ExceptionDate = source.ExceptionDate,
            ExceptionType = source.ExceptionType == CalendarStatus.WorkingDay ? "WorkingDayOverride" : "HolidayOverride",
            Name = source.Reason,
            Description = "",
            RelatedHolidayId = source.RelatedHolidayId?.Value,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public CalendarExceptionIdResponse ToCalendarExceptionIdResponse(CalendarException source)
    {
        return new CalendarExceptionIdResponse
        {
            Id = source.Id.Value,
            CreatedAt = source.CreatedAt
        };
    }
}
