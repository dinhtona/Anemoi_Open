using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.ShiftManagement;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class ShiftTemplateModelMapping : IEntityTypeConfiguration<ShiftTemplate>
{
    public void Configure(EntityTypeBuilder<ShiftTemplate> builder)
    {
        builder.ToTable("ShiftTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new ShiftTemplateId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.StartTime)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(x => x.BreakMinutes)
            .IsRequired();

        builder.Property(x => x.ExpectedWorkingHours)
            .HasPrecision(9, 2)
            .IsRequired();

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

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}

public sealed class EmployeeShiftAssignmentModelMapping : IEntityTypeConfiguration<EmployeeShiftAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeShiftAssignment> builder)
    {
        builder.ToTable("EmployeeShiftAssignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeShiftAssignmentId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.ShiftTemplateId)
            .HasConversion(x => x.Value, id => new ShiftTemplateId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.WorkDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.ShiftNameSnapshot)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.StartTimeSnapshot)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(x => x.EndTimeSnapshot)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(x => x.BreakMinutesSnapshot)
            .IsRequired();

        builder.Property(x => x.ExpectedWorkingHoursSnapshot)
            .HasPrecision(9, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.AssignedBy)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.AssignedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.CancelledBy)
            .HasMaxLength(128)
            .IsRequired(false);

        builder.Property(x => x.CancelledAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500)
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

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ShiftTemplate)
            .WithMany()
            .HasForeignKey(x => x.ShiftTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmployeeId, x.WorkDate, x.Status });
        builder.HasIndex(x => x.ShiftTemplateId);
        builder.HasIndex(x => x.WorkDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
    }
}
