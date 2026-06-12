using Anemoi.Hr.Domain.CalendarManagement;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class PublicHolidayModelMapping : IEntityTypeConfiguration<PublicHoliday>
{
    public void Configure(EntityTypeBuilder<PublicHoliday> builder)
    {
        builder.ToTable("PublicHolidays");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new PublicHolidayId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.HolidayDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.CountryCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.IsRecurringAnnual)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(x => new { x.HolidayDate, x.CountryCode }).IsUnique();
    }
}

public sealed class CompanyHolidayModelMapping : IEntityTypeConfiguration<CompanyHoliday>
{
    public void Configure(EntityTypeBuilder<CompanyHoliday> builder)
    {
        builder.ToTable("CompanyHolidays");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new CompanyHolidayId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.HolidayDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.IsRecurringAnnual)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }
}

public sealed class WorkingCalendarRuleModelMapping : IEntityTypeConfiguration<WorkingCalendarRule>
{
    public void Configure(EntityTypeBuilder<WorkingCalendarRule> builder)
    {
        builder.ToTable("WorkingCalendarRules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkingCalendarRuleId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.WorkMonday)
            .IsRequired();

        builder.Property(x => x.WorkTuesday)
            .IsRequired();

        builder.Property(x => x.WorkWednesday)
            .IsRequired();

        builder.Property(x => x.WorkThursday)
            .IsRequired();

        builder.Property(x => x.WorkFriday)
            .IsRequired();

        builder.Property(x => x.WorkSaturday)
            .IsRequired();

        builder.Property(x => x.WorkSunday)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.EffectiveTo)
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(x => new { x.EffectiveFrom, x.EffectiveTo, x.IsActive });
    }
}

public sealed class CalendarExceptionModelMapping : IEntityTypeConfiguration<CalendarException>
{
    public void Configure(EntityTypeBuilder<CalendarException> builder)
    {
        builder.ToTable("CalendarExceptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new CalendarExceptionId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.ExceptionDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.ExceptionType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.RelatedHolidayId)
            .HasConversion(x => x.Value, id => new PublicHolidayId(id))
            .HasColumnType("uuid")
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(x => x.ExceptionDate);
    }
}
