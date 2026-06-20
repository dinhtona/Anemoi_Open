using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.Domain.MasterData;
using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class LeaveTypeModelMapping : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new LeaveTypeId(id));
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.AnnualEntitlement).HasPrecision(9, 2);
        builder.Property(x => x.MaxCarryForwardDays).HasPrecision(9, 2);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class MasterDataLeavePolicyModelMapping : IEntityTypeConfiguration<Anemoi.Hr.Domain.MasterData.LeavePolicy>
{
    public void Configure(EntityTypeBuilder<Anemoi.Hr.Domain.MasterData.LeavePolicy> builder)
    {
        builder.ToTable("MasterDataLeavePolicies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new LeavePolicyId(id));
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.LeaveTypeId).HasConversion(x => x.Value, id => new LeaveTypeId(id));
        builder.Property(x => x.ApplicableGradeCode).HasMaxLength(64);
        builder.Property(x => x.AnnualEntitlement).HasPrecision(9, 2);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.LeaveTypeId, x.ApplicableGradeCode }).IsUnique();
        builder.HasOne(x => x.LeaveType)
            .WithMany()
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class OvertimeRuleModelMapping : IEntityTypeConfiguration<OvertimeRule>
{
    public void Configure(EntityTypeBuilder<OvertimeRule> builder)
    {
        builder.ToTable("OvertimeRules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new OvertimeRuleId(id));
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.WeekdayMultiplier).HasPrecision(5, 2);
        builder.Property(x => x.WeekendMultiplier).HasPrecision(5, 2);
        builder.Property(x => x.HolidayMultiplier).HasPrecision(5, 2);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}
