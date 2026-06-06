using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class PayrollModelMapping :
    IEntityTypeConfiguration<PayrollPeriod>,
    IEntityTypeConfiguration<PayrollRun>,
    IEntityTypeConfiguration<PayrollItem>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        builder.ToTable("PayrollPeriods");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new PayrollPeriodId(id));

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

    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("PayrollRuns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new PayrollRunId(id));

        builder.Property(x => x.PayrollPeriodId)
            .HasConversion(x => x.Value, id => new PayrollPeriodId(id))
            .IsRequired();

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .IsRequired();

        builder.Property(x => x.EmployeeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EmployeeName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
        builder.Property(x => x.PayScheduleType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CalculatedBy).HasMaxLength(128).IsRequired();

        builder.Property(x => x.BaseSalary).HasPrecision(18, 2);
        builder.Property(x => x.TotalAllowanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.GrossAmount).HasPrecision(18, 2);

        // Unique Index: PayrollPeriodId + EmployeeId
        builder.HasIndex(x => new { x.PayrollPeriodId, x.EmployeeId }).IsUnique();

        // Relationships
        builder.HasOne(x => x.PayrollPeriod)
            .WithMany()
            .HasForeignKey(x => x.PayrollPeriodId)
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

    public void Configure(EntityTypeBuilder<PayrollItem> builder)
    {
        builder.ToTable("PayrollItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new PayrollItemId(id));

        builder.Property(x => x.PayrollRunId)
            .HasConversion(x => x.Value, id => new PayrollRunId(id))
            .IsRequired();

        builder.Property(x => x.ItemCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ItemName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        // Index on PayrollRunId
        builder.HasIndex(x => x.PayrollRunId);

        // Relationship
        builder.HasOne(x => x.PayrollRun)
            .WithMany(x => x.PayrollItems)
            .HasForeignKey(x => x.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);

        // PostgreSQL xmin concurrency
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }
}
