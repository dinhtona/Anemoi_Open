using Anemoi.Hr.Domain.Onboarding;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class OnboardingPlanTemplateMapping : IEntityTypeConfiguration<OnboardingPlanTemplate>
{
    public void Configure(EntityTypeBuilder<OnboardingPlanTemplate> builder)
    {
        builder.ToTable("OnboardingPlanTemplates", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new OnboardingPlanTemplateId(id));

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Version).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(200).IsRequired();

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasMany(x => x.TaskTemplates)
            .WithOne()
            .HasForeignKey("TemplateId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class OnboardingTaskTemplateMapping : IEntityTypeConfiguration<OnboardingTaskTemplate>
{
    public void Configure(EntityTypeBuilder<OnboardingTaskTemplate> builder)
    {
        builder.ToTable("OnboardingTaskTemplates", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new OnboardingTaskTemplateId(id));

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.AssigneeType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AssigneeRoleCode).HasMaxLength(50);
        builder.Property(x => x.OffsetDays).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsRequired).IsRequired();
    }
}

public sealed class OnboardingInstanceMapping : IEntityTypeConfiguration<OnboardingInstance>
{
    public void Configure(EntityTypeBuilder<OnboardingInstance> builder)
    {
        builder.ToTable("OnboardingInstances", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new OnboardingInstanceId(id));

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.TemplateId)
            .HasConversion(x => x!.Value, id => new OnboardingPlanTemplateId(id));
        builder.Property(x => x.TemplateName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CompletedBy).HasMaxLength(200);
        builder.Property(x => x.CancelledBy).HasMaxLength(200);
        builder.Property(x => x.ForceCompletedBy).HasMaxLength(200);
        builder.Property(x => x.ForceCompleteReason).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(200).IsRequired();

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        builder.HasIndex("EmployeeId")
            .IsUnique()
            .HasFilter("\"Status\" IN ('Draft', 'InProgress')");
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.StartDate);

        builder.HasMany(x => x.Tasks)
            .WithOne()
            .HasForeignKey("InstanceId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class OnboardingTaskMapping : IEntityTypeConfiguration<OnboardingTask>
{
    public void Configure(EntityTypeBuilder<OnboardingTask> builder)
    {
        builder.ToTable("OnboardingTasks", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new OnboardingTaskId(id));

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.AssigneeType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AssigneeRoleCode).HasMaxLength(50);
        builder.Property(x => x.AssignedUserId).HasMaxLength(200);
        builder.Property(x => x.AssignedUserDisplayNameSnapshot).HasMaxLength(300);
        builder.Property(x => x.AssignedBy).HasMaxLength(200);
        builder.Property(x => x.ReassignedBy).HasMaxLength(200);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CompletedBy).HasMaxLength(200);
        builder.Property(x => x.CompletedNotes).HasMaxLength(500);
        builder.Property(x => x.SkippedBy).HasMaxLength(200);
        builder.Property(x => x.ReopenedBy).HasMaxLength(200);
        builder.Property(x => x.ReopenedReason).HasMaxLength(500);

        builder.HasIndex(x => new { x.AssignedUserId, x.Status });
        builder.HasIndex("DueDate").HasFilter("\"Status\" = 'Pending'");
        builder.HasIndex("InstanceId");
    }
}
