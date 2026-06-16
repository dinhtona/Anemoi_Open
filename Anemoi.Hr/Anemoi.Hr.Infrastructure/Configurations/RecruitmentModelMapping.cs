using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class RecruitmentModelMapping :
    IEntityTypeConfiguration<JobRequisition>,
    IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobRequisition> builder)
    {
        builder.ToTable("JobRequisitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new JobRequisitionId(id));

        builder.Property(x => x.RequisitionCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Headcount).IsRequired();
        builder.Property(x => x.EmploymentType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RequestedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ApprovedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.OpenDate).IsRequired();
        builder.Property(x => x.TargetHireDate).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2048).IsRequired(false);

        builder.Property(x => x.Status).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(128).IsRequired();

        builder.Property(x => x.DepartmentId)
            .HasConversion(x => x.Value, id => new DepartmentId(id))
            .IsRequired();

        builder.Property(x => x.PositionId)
            .HasConversion(x => x.Value, id => new PositionId(id))
            .IsRequired();

        builder.Property(x => x.SubmittedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.SubmittedAt).IsRequired(false);
        builder.Property(x => x.ApprovedAt).IsRequired(false);
        builder.Property(x => x.RejectedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.RejectedAt).IsRequired(false);
        builder.Property(x => x.RejectionReason).IsRequired(false);
        builder.Property(x => x.ClosedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.ClosedAt).IsRequired(false);
        builder.Property(x => x.CancelledBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.CancelledAt).IsRequired(false);
        builder.Property(x => x.CancellationReason).IsRequired(false);

        // Unique Index
        builder.HasIndex(x => x.RequisitionCode).IsUnique();

        // Relationships
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        // PostgreSQL xmin concurrency
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }

    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.ToTable("JobPostings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new JobPostingId(id));

        builder.Property(x => x.JobRequisitionId)
            .HasConversion(x => x.Value, id => new JobRequisitionId(id))
            .IsRequired();

        builder.Property(x => x.PostingTitle).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PostingDescription).HasMaxLength(4096).IsRequired(false);
        builder.Property(x => x.PublishDate).IsRequired();
        builder.Property(x => x.ExpiryDate).IsRequired();

        builder.Property(x => x.Status).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(128).IsRequired();

        builder.Property(x => x.PublishedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.PublishedAt).IsRequired(false);
        builder.Property(x => x.ExpiredBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.ExpiredAt).IsRequired(false);
        builder.Property(x => x.ClosedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.ClosedAt).IsRequired(false);

        // Relationships
        builder.HasOne(x => x.JobRequisition)
            .WithMany()
            .HasForeignKey(x => x.JobRequisitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index on JobRequisitionId for lookups
        builder.HasIndex(x => x.JobRequisitionId);

        // Unique Index on PostingTitle per Requisition (optional safety)
        builder.HasIndex(x => new { x.JobRequisitionId, x.PostingTitle });

        // PostgreSQL xmin concurrency
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }
}
