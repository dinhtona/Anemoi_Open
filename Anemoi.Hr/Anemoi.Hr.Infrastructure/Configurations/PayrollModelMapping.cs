using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Attendance;
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
        
        builder.Property(x => x.StandardWorkingDays).HasPrecision(9, 2).IsRequired();

        builder.Property(x => x.AttendancePeriodId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new AttendancePeriodId(value.Value) : null)
            .IsRequired(false);

        // Relationship
        builder.HasOne(x => x.AttendancePeriod)
            .WithMany()
            .HasForeignKey(x => x.AttendancePeriodId)
            .OnDelete(DeleteBehavior.Restrict);

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

        builder.Property(x => x.DepartmentIdSnapshot)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new DepartmentId(value.Value) : null)
            .IsRequired(false);

        builder.Property(x => x.DepartmentNameSnapshot)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.CalculatedBy).HasMaxLength(128).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(s => s.ToString(), v => (PayrollRunStatus)Enum.Parse(typeof(PayrollRunStatus), v))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.SubmittedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.SubmittedAt).IsRequired(false);
        builder.Property(x => x.ApprovedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.ApprovedAt).IsRequired(false);
        builder.Property(x => x.RejectedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.RejectedAt).IsRequired(false);
        builder.Property(x => x.RejectionReason).IsRequired(false);
        builder.Property(x => x.FinalizedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.FinalizedAt).IsRequired(false);
        builder.Property(x => x.CancelledBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.CancelledAt).IsRequired(false);
        builder.Property(x => x.CancellationReason).IsRequired(false);

        builder.Property(x => x.BaseSalary).HasPrecision(18, 2);
        builder.Property(x => x.TotalAllowanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.GrossAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.StandardWorkingDays).HasPrecision(9, 2).IsRequired();
        builder.Property(x => x.PaidWorkingDays).HasPrecision(9, 2).IsRequired();
        builder.Property(x => x.UnpaidLeaveDays).HasPrecision(9, 2).IsRequired();
        builder.Property(x => x.DailyRate).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.BasePayAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.TotalDeductionAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.NetAmount).HasPrecision(18, 2).IsRequired();

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
        builder.Property(x => x.ItemTypeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.Property(x => x.AttendanceSummaryId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new AttendanceSummaryId(value.Value) : null)
            .IsRequired(false);

        builder.Property(x => x.PaidWorkingDays).HasPrecision(9, 2).IsRequired();
        builder.Property(x => x.PaidLeaveDays).HasPrecision(9, 2).IsRequired();
        builder.Property(x => x.UnpaidLeaveDays).HasPrecision(9, 2).IsRequired();
        builder.Property(x => x.BaseSalarySnapshot).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.DailyRateSnapshot).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.BasePayAmount).HasPrecision(18, 2).IsRequired();

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

public sealed class PayslipModelMapping : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.ToTable("Payslips");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new PayslipId(id));

        builder.Property(x => x.PayrollRunId)
            .HasConversion(x => x.Value, id => new PayrollRunId(id))
            .IsRequired();

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .IsRequired();

        builder.Property(x => x.PeriodCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EmployeeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EmployeeName).HasMaxLength(256).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(s => s.ToString(), v => (PayslipStatus)Enum.Parse(typeof(PayslipStatus), v))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.BaseSalarySnapshot).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.DailyRateSnapshot).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.PaidWorkingDays).HasPrecision(9, 2).IsRequired();
        builder.Property(x => x.PaidLeaveDays).HasPrecision(9, 2).IsRequired();
        builder.Property(x => x.UnpaidLeaveDays).HasPrecision(9, 2).IsRequired();
        builder.Property(x => x.BasePayAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.AllowanceTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.DeductionTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.GrossPay).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.NetPay).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.GeneratedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PublishedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.CancelledBy).HasMaxLength(128).IsRequired(false);

        // Unique Index: PayrollRunId + EmployeeId
        builder.HasIndex(x => new { x.PayrollRunId, x.EmployeeId }).IsUnique();

        // Relationships
        builder.HasOne(x => x.PayrollRun)
            .WithMany()
            .HasForeignKey(x => x.PayrollRunId)
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
