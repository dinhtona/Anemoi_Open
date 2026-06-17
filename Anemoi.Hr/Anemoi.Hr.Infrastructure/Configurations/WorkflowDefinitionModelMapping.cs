using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class WorkflowDefinitionModelMapping : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("WorkflowDefinitions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowDefinitionId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.WorkflowTypeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TargetEntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.WorkflowTypeCode);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => new { x.TargetEntityType, x.IsActive });

        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey(x => x.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WorkflowDefinitionStepModelMapping : IEntityTypeConfiguration<WorkflowDefinitionStep>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinitionStep> builder)
    {
        builder.ToTable("WorkflowDefinitionSteps");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowDefinitionStepId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.WorkflowDefinitionId)
            .HasConversion(x => x.Value, id => new WorkflowDefinitionId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.ApproverType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ApproverValue).HasMaxLength(256).IsRequired(false);
        builder.Property(x => x.IsRequired).IsRequired();

        builder.HasIndex(x => new { x.WorkflowDefinitionId, x.Sequence }).IsUnique();
    }
}
