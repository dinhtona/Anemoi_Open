using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class InsuranceRuleSetModelMapping : IEntityTypeConfiguration<InsuranceRuleSet>
{
    public void Configure(EntityTypeBuilder<InsuranceRuleSet> builder)
    {
        builder.ToTable("InsuranceRuleSets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new InsuranceRuleSetId(id));

        builder.Property(x => x.CountryCode).HasMaxLength(16).IsRequired();
        builder.Property(x => x.InsuranceType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(x => new { x.CountryCode, x.InsuranceType, x.EffectiveFrom, x.EffectiveTo });
        builder.HasIndex(x => new { x.CountryCode, x.InsuranceType, x.Status });

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class InsuranceContributionRuleModelMapping : IEntityTypeConfiguration<InsuranceContributionRule>
{
    public void Configure(EntityTypeBuilder<InsuranceContributionRule> builder)
    {
        builder.ToTable("InsuranceContributionRules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new InsuranceContributionRuleId(id));
        builder.Property(x => x.RuleSetId).HasConversion(x => x.Value, id => new InsuranceRuleSetId(id));

        builder.Property(x => x.ContributionType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SalaryBasis).HasMaxLength(64).IsRequired();

        builder.HasOne(x => x.RuleSet)
            .WithMany(x => x.ContributionRules)
            .HasForeignKey(x => x.RuleSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.RuleSetId, x.SortOrder });
        builder.HasIndex(x => new { x.RuleSetId, x.ContributionType });

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class InsuranceCalculationSnapshotModelMapping : IEntityTypeConfiguration<InsuranceCalculationSnapshot>
{
    public void Configure(EntityTypeBuilder<InsuranceCalculationSnapshot> builder)
    {
        builder.ToTable("InsuranceCalculationSnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new InsuranceCalculationSnapshotId(id));
        builder.Property(x => x.EmployeeId).HasConversion(x => x!.Value, id => new EmployeeId(id));
        builder.Property(x => x.RuleSetId).HasConversion(x => x.Value, id => new InsuranceRuleSetId(id));

        builder.Property(x => x.CountryCode).HasMaxLength(16).IsRequired();
        builder.Property(x => x.InsuranceType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(16).IsRequired();
        builder.Property(x => x.CalculatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SourceModule).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SourceReferenceId).HasMaxLength(128);

        builder.Property(x => x.RuleSetSnapshotJson).HasColumnType("text").IsRequired();
        builder.Property(x => x.CalculationResultJson).HasColumnType("text");

        builder.HasIndex(x => new { x.EmployeeId, x.CalculationPeriodStart, x.CalculationPeriodEnd });
        builder.HasIndex(x => new { x.SourceModule, x.SourceReferenceId });

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class InsuranceCalculationSnapshotItemModelMapping : IEntityTypeConfiguration<InsuranceCalculationSnapshotItem>
{
    public void Configure(EntityTypeBuilder<InsuranceCalculationSnapshotItem> builder)
    {
        builder.ToTable("InsuranceCalculationSnapshotItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new InsuranceCalculationSnapshotItemId(id));
        builder.Property(x => x.SnapshotId).HasConversion(x => x.Value, id => new InsuranceCalculationSnapshotId(id));

        builder.Property(x => x.InsuranceType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ContributionType).HasMaxLength(64).IsRequired();

        builder.HasOne(x => x.Snapshot)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SnapshotId, x.SortOrder });

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class InsuranceAuditLogModelMapping : IEntityTypeConfiguration<InsuranceAuditLog>
{
    public void Configure(EntityTypeBuilder<InsuranceAuditLog> builder)
    {
        builder.ToTable("InsuranceAuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new InsuranceAuditLogId(id));
        builder.Property(x => x.RuleSetId).HasConversion(x => x!.Value, id => new InsuranceRuleSetId(id));

        builder.Property(x => x.Action).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.PerformedBy).HasMaxLength(128).IsRequired();

        builder.HasOne(x => x.RuleSet)
            .WithMany()
            .HasForeignKey(x => x.RuleSetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.RuleSetId);

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}
