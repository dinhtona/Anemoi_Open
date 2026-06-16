using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class RecruitmentModelMapping :
    IEntityTypeConfiguration<JobRequisition>,
    IEntityTypeConfiguration<JobPosting>,
    IEntityTypeConfiguration<Candidate>,
    IEntityTypeConfiguration<CandidateApplication>,
    IEntityTypeConfiguration<CandidateApplicationStageHistory>
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

    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("Candidates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new CandidateId(id));

        builder.Property(x => x.CandidateCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(32).IsRequired(false);
        builder.Property(x => x.DateOfBirth).IsRequired(false);
        builder.Property(x => x.Address).HasMaxLength(1024).IsRequired(false);
        builder.Property(x => x.ResumeUrl).HasMaxLength(2048).IsRequired(false);
        builder.Property(x => x.Source).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(4096).IsRequired(false);
        builder.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ConvertedBy).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.ConvertedAt).IsRequired(false);

        builder.Property(x => x.EmployeeId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new EmployeeId(value.Value) : null)
            .IsRequired(false);

        // Unique indexes
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.CandidateCode).IsUnique();
        builder.HasIndex(x => x.PhoneNumber)
            .IsUnique()
            .HasFilter("\"PhoneNumber\" IS NOT NULL AND \"PhoneNumber\" <> ''");

        // Relationship
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // PostgreSQL xmin concurrency
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }

    public void Configure(EntityTypeBuilder<CandidateApplication> builder)
    {
        builder.ToTable("CandidateApplications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new CandidateApplicationId(id));

        builder.Property(x => x.CandidateId)
            .HasConversion(x => x.Value, id => new CandidateId(id))
            .IsRequired();

        builder.Property(x => x.JobPostingId)
            .HasConversion(x => x.Value, id => new JobPostingId(id))
            .IsRequired();

        builder.Property(x => x.CurrentStage).HasMaxLength(64).IsRequired();
        builder.Property(x => x.AppliedAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Unique index: one application per candidate per posting
        builder.HasIndex(x => new { x.CandidateId, x.JobPostingId }).IsUnique();

        // Indexes for search
        builder.HasIndex(x => x.JobPostingId);
        builder.HasIndex(x => x.CandidateId);
        builder.HasIndex(x => x.CurrentStage);
        builder.HasIndex(x => x.AppliedAt);

        // Relationships
        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.JobPosting)
            .WithMany()
            .HasForeignKey(x => x.JobPostingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.StageHistories)
            .WithOne(x => x.CandidateApplication)
            .HasForeignKey(x => x.CandidateApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // PostgreSQL xmin concurrency
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }

    public void Configure(EntityTypeBuilder<CandidateApplicationStageHistory> builder)
    {
        builder.ToTable("CandidateApplicationStageHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new CandidateApplicationStageHistoryId(id));

        builder.Property(x => x.CandidateApplicationId)
            .HasConversion(x => x.Value, id => new CandidateApplicationId(id))
            .IsRequired();

        builder.Property(x => x.FromStage).HasMaxLength(64).IsRequired(false);
        builder.Property(x => x.ToStage).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ChangedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ChangedAt).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(1024).IsRequired(false);

        builder.HasIndex(x => x.CandidateApplicationId);
    }
}
