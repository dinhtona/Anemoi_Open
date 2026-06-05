using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class LeaveModelMapping :
    IEntityTypeConfiguration<LeavePolicy>,
    IEntityTypeConfiguration<LeaveBalance>,
    IEntityTypeConfiguration<LeaveRequest>,
    IEntityTypeConfiguration<LeaveTransaction>,
    IEntityTypeConfiguration<LeaveAccrualRun>
{
    public void Configure(EntityTypeBuilder<LeavePolicy> builder)
    {
        builder.ToTable("LeavePolicies");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new LeavePolicyId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.LeaveTypeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.MonthlyAccrualDays).HasPrecision(9, 2);
        builder.Property(x => x.AnnualMaxDays).HasPrecision(9, 2);
        builder.Property(x => x.MaxCarryForwardDays).HasPrecision(9, 2);
        builder.HasIndex(x => x.Code).IsUnique();
    }

    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new LeaveBalanceId(id));
        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.LeavePolicyId)
            .HasConversion(x => x.Value, id => new LeavePolicyId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OpeningDays).HasPrecision(9, 2);
        builder.Property(x => x.AccruedDays).HasPrecision(9, 2);
        builder.Property(x => x.UsedDays).HasPrecision(9, 2);
        builder.Property(x => x.PendingDays).HasPrecision(9, 2);
        builder.Property(x => x.AdjustedDays).HasPrecision(9, 2);
        builder.Property(x => x.RemainingDays).HasPrecision(9, 2);
        builder.HasIndex(x => new { x.EmployeeId, x.LeavePolicyId, x.Year }).IsUnique();
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeavePolicy)
            .WithMany(x => x.LeaveBalances)
            .HasForeignKey(x => x.LeavePolicyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }

    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new LeaveRequestId(id));
        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.LeavePolicyId)
            .HasConversion(x => x.Value, id => new LeavePolicyId(id));
        builder.Property(x => x.ApproverEmployeeId)
            .HasConversion(x => x == null ? default(Guid?) : x.Value, id => id == null ? null : new EmployeeId(id.Value));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LeaveTypeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RequestedDays).HasPrecision(9, 2);
        builder.Property(x => x.StatusCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1024);
        builder.HasIndex(x => new { x.EmployeeId, x.StatusCode });
        builder.HasIndex(x => new { x.StartDate, x.EndDate });
        builder.HasIndex(x => new { x.StatusCode, x.StartDate, x.EndDate });
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ApproverEmployee)
            .WithMany()
            .HasForeignKey(x => x.ApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeavePolicy)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.LeavePolicyId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<LeaveTransaction> builder)
    {
        builder.ToTable("LeaveTransactions");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new LeaveTransactionId(id));
        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.LeavePolicyId)
            .HasConversion(x => x.Value, id => new LeavePolicyId(id));
        builder.Property(x => x.LeaveBalanceId)
            .HasConversion(x => x.Value, id => new LeaveBalanceId(id));
        builder.Property(x => x.LeaveRequestId)
            .HasConversion(x => x == null ? default(Guid?) : x.Value, id => id == null ? null : new LeaveRequestId(id.Value));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TransactionTypeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Days).HasPrecision(9, 2);
        builder.Property(x => x.BalanceAfterDays).HasPrecision(9, 2);
        builder.Property(x => x.SourceType).HasMaxLength(64);
        builder.Property(x => x.SourceId).HasMaxLength(128);
        builder.Property(x => x.Reason).HasMaxLength(1024);
        builder.HasIndex(x => new { x.EmployeeId, x.CreatedAt });
        builder.HasIndex(x => x.LeaveBalanceId);
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeavePolicy)
            .WithMany(x => x.LeaveTransactions)
            .HasForeignKey(x => x.LeavePolicyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeaveBalance)
            .WithMany(x => x.LeaveTransactions)
            .HasForeignKey(x => x.LeaveBalanceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeaveRequest)
            .WithMany(x => x.LeaveTransactions)
            .HasForeignKey(x => x.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<LeaveAccrualRun> builder)
    {
        builder.ToTable("LeaveAccrualRuns");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new LeaveAccrualRunId(id));
        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.LeavePolicyId)
            .HasConversion(x => x.Value, id => new LeavePolicyId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.YearMonth).HasMaxLength(7).IsRequired();
        builder.Property(x => x.AccruedDays).HasPrecision(9, 2);
        builder.HasIndex(x => new { x.EmployeeId, x.LeavePolicyId, x.YearMonth }).IsUnique();
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeavePolicy)
            .WithMany(x => x.LeaveAccrualRuns)
            .HasForeignKey(x => x.LeavePolicyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
