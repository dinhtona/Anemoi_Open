using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class WorkflowInstanceModelMapping : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowInstanceId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.WorkflowDefinitionId)
            .HasConversion(x => x.Value, id => new WorkflowDefinitionId(id))
            .HasColumnType("uuid").IsRequired(false);

        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CurrentStep).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StartedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RequesterEmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .IsRequired(false);
        builder.Property(x => x.RequesterUserId)
            .HasConversion(x => x.Value, id => new UserId(id))
            .HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.CompletedAt).IsRequired(false);

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.WorkflowDefinitionId);
        builder.HasIndex(x => x.StartedBy);

        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey(x => x.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Histories)
            .WithOne()
            .HasForeignKey(x => x.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WorkflowInstanceStepModelMapping : IEntityTypeConfiguration<WorkflowInstanceStep>
{
    public void Configure(EntityTypeBuilder<WorkflowInstanceStep> builder)
    {
        builder.ToTable("WorkflowInstanceSteps");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowInstanceStepId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.WorkflowInstanceId)
            .HasConversion(x => x.Value, id => new WorkflowInstanceId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.ApproverTypeSnapshot).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ApproverValueSnapshot).HasMaxLength(256).IsRequired(false);
        builder.Property(x => x.ApproverUserId).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.ApproverEmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .HasColumnType("uuid").IsRequired(false);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ApprovedAt).IsRequired(false);
        builder.Property(x => x.RejectedAt).IsRequired(false);
        builder.Property(x => x.Comment).HasMaxLength(1000).IsRequired(false);

        builder.HasIndex(x => x.WorkflowInstanceId);
        builder.HasIndex(x => x.Status);
    }
}

public sealed class WorkflowRoleAssignmentModelMapping : IEntityTypeConfiguration<WorkflowRoleAssignment>
{
    public void Configure(EntityTypeBuilder<WorkflowRoleAssignment> builder)
    {
        builder.ToTable("WorkflowRoleAssignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowRoleAssignmentId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.Role).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .HasColumnType("uuid").IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Role);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => new { x.Role, x.EmployeeId }).IsUnique();
    }
}

public sealed class WorkflowHistoryModelMapping : IEntityTypeConfiguration<WorkflowHistory>
{
    public void Configure(EntityTypeBuilder<WorkflowHistory> builder)
    {
        builder.ToTable("WorkflowHistories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowHistoryId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.WorkflowInstanceId)
            .HasConversion(x => x.Value, id => new WorkflowInstanceId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PerformedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.PerformedAt).IsRequired();

        builder.HasIndex(x => x.WorkflowInstanceId);
        builder.HasIndex(x => x.PerformedAt);
    }
}
