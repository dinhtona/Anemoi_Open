using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class TaxRuleSetModelMapping : IEntityTypeConfiguration<TaxRuleSet>
{
    public void Configure(EntityTypeBuilder<TaxRuleSet> builder)
    {
        builder.ToTable("hr_tax_rule_sets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new TaxRuleSetId(id));

        builder.Property(x => x.CountryCode).HasMaxLength(16).IsRequired();
        builder.Property(x => x.TaxType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(x => new { x.CountryCode, x.TaxType, x.EffectiveFrom, x.EffectiveTo });

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class TaxBracketModelMapping : IEntityTypeConfiguration<TaxBracket>
{
    public void Configure(EntityTypeBuilder<TaxBracket> builder)
    {
        builder.ToTable("hr_tax_brackets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new TaxBracketId(id));
        builder.Property(x => x.TaxRuleSetId).HasConversion(x => x.Value, id => new TaxRuleSetId(id));

        builder.HasOne(x => x.TaxRuleSet)
            .WithMany(x => x.Brackets)
            .HasForeignKey(x => x.TaxRuleSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TaxRuleSetId, x.SortOrder });

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class TaxDeductionRuleModelMapping : IEntityTypeConfiguration<TaxDeductionRule>
{
    public void Configure(EntityTypeBuilder<TaxDeductionRule> builder)
    {
        builder.ToTable("hr_tax_deduction_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new TaxDeductionRuleId(id));
        builder.Property(x => x.TaxRuleSetId).HasConversion(x => x.Value, id => new TaxRuleSetId(id));
        builder.Property(x => x.DeductionType).HasMaxLength(64).IsRequired();

        builder.HasOne(x => x.TaxRuleSet)
            .WithMany(x => x.DeductionRules)
            .HasForeignKey(x => x.TaxRuleSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TaxRuleSetId, x.DeductionType });

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class TaxCalculationSnapshotModelMapping : IEntityTypeConfiguration<TaxCalculationSnapshot>
{
    public void Configure(EntityTypeBuilder<TaxCalculationSnapshot> builder)
    {
        builder.ToTable("hr_tax_calculation_snapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new TaxCalculationSnapshotId(id));
        builder.Property(x => x.EmployeeId).HasConversion(x => x!.Value, id => new EmployeeId(id));
        builder.Property(x => x.PayrollRunId).HasConversion(x => x!.Value, id => new PayrollRunId(id));
        builder.Property(x => x.TaxRuleSetId).HasConversion(x => x.Value, id => new TaxRuleSetId(id));

        builder.Property(x => x.CountryCode).HasMaxLength(16).IsRequired();
        builder.Property(x => x.TaxType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(16).IsRequired();
        builder.Property(x => x.CalculatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SourceModule).HasMaxLength(64).IsRequired();

        builder.Property(x => x.DeductionSnapshotJson).HasColumnType("text");
        builder.Property(x => x.RuleSetSnapshotJson).HasColumnType("text").IsRequired();
        builder.Property(x => x.BracketSnapshotJson).HasColumnType("text");
        builder.Property(x => x.CalculationResultJson).HasColumnType("text");

        builder.HasIndex(x => new { x.EmployeeId, x.CalculationPeriodStart, x.CalculationPeriodEnd });
        builder.HasIndex(x => new { x.SourceModule, x.SourceReferenceId });

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}
