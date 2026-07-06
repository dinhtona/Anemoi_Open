using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class OvertimeRequestModelMapping : IEntityTypeConfiguration<OvertimeRequest>
{
    public void Configure(EntityTypeBuilder<OvertimeRequest> builder)
    {
        builder.ToTable("OvertimeRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new OvertimeRequestId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.OvertimeDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.StartTime)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ApprovedBy)
            .HasMaxLength(128)
            .IsRequired(false);

        builder.Property(x => x.ApprovedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(x => x.RejectedBy)
            .HasMaxLength(128)
            .IsRequired(false);

        builder.Property(x => x.RejectedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(x => x.CancelledBy)
            .HasMaxLength(128)
            .IsRequired(false);

        builder.Property(x => x.CancelledAt)
            .HasColumnType("timestamp with time zone")
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

        builder.HasIndex(x => new { x.EmployeeId, x.OvertimeDate });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.OvertimeDate);
        builder.HasIndex(x => x.CreatedAt);
    }
}
