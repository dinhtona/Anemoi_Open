using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class AttendanceModelMapping :
    IEntityTypeConfiguration<AttendancePeriod>,
    IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendancePeriod> builder)
    {
        builder.ToTable("AttendancePeriods");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new AttendancePeriodId(id));

        builder.Property(x => x.PeriodCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(128).IsRequired();

        // Unique Index
        builder.HasIndex(x => x.PeriodCode).IsUnique();

        // PostgreSQL xmin concurrency
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }

    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new AttendanceRecordId(id));

        builder.Property(x => x.AttendancePeriodId)
            .HasConversion(x => x.Value, id => new AttendancePeriodId(id))
            .IsRequired();

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .IsRequired();

        builder.Property(x => x.Status).HasMaxLength(64).IsRequired();

        builder.Property(x => x.LeaveRequestId)
            .HasConversion(
                x => x == null ? default(Guid?) : x.Value,
                id => id == null ? null : new LeaveRequestId(id.Value));

        builder.Property(x => x.WorkedHours).HasPrecision(9, 2).IsRequired();
        builder.Property(x => x.WorkedDays).HasPrecision(9, 2).IsRequired();

        builder.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(128).IsRequired();

        // Unique Index
        builder.HasIndex(x => new { x.EmployeeId, x.WorkDate }).IsUnique();

        // Indexes
        builder.HasIndex(x => x.AttendancePeriodId);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.WorkDate);

        // Relationships
        builder.HasOne(x => x.AttendancePeriod)
            .WithMany()
            .HasForeignKey(x => x.AttendancePeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // PostgreSQL xmin concurrency
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }
}
