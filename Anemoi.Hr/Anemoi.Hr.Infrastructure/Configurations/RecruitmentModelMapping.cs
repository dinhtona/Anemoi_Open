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
    IEntityTypeConfiguration<CandidateApplicationStageHistory>,
    IEntityTypeConfiguration<InterviewSchedule>,
    IEntityTypeConfiguration<InterviewFeedback>,
    IEntityTypeConfiguration<HiringDecision>,
    IEntityTypeConfiguration<RecruitmentRequest>,
    IEntityTypeConfiguration<RecruitmentRequestHistory>,
    IEntityTypeConfiguration<RecruitmentOpening>
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

        builder.Property(x => x.RecruitmentOpeningId)
            .HasConversion(x => x.Value, id => new RecruitmentOpeningId(id))
            .IsRequired(false);

        builder.HasOne(x => x.RecruitmentOpening)
            .WithMany()
            .HasForeignKey(x => x.RecruitmentOpeningId)
            .OnDelete(DeleteBehavior.SetNull);

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

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }

    public void Configure(EntityTypeBuilder<InterviewSchedule> builder)
    {
        builder.ToTable("InterviewSchedules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new InterviewScheduleId(id));

        builder.Property(x => x.CandidateApplicationId)
            .HasConversion(x => x.Value, id => new CandidateApplicationId(id))
            .IsRequired();

        builder.Property(x => x.InterviewType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ScheduledAt).IsRequired();
        builder.Property(x => x.DurationMinutes).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2048).IsRequired(false);

        builder.Property(x => x.Result).HasMaxLength(64).IsRequired();

        builder.Property(x => x.InterviewerEmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .IsRequired();

        builder.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(128).IsRequired();

        builder.HasIndex(x => x.CandidateApplicationId);
        builder.HasIndex(x => x.ScheduledAt);
        builder.HasIndex(x => x.InterviewerEmployeeId);
        builder.HasIndex(x => x.Result);

        builder.HasOne(x => x.CandidateApplication)
            .WithMany()
            .HasForeignKey(x => x.CandidateApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Interviewer)
            .WithMany()
            .HasForeignKey(x => x.InterviewerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Feedbacks)
            .WithOne(x => x.InterviewSchedule)
            .HasForeignKey(x => x.InterviewScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }

    public void Configure(EntityTypeBuilder<InterviewFeedback> builder)
    {
        builder.ToTable("InterviewFeedbacks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new InterviewFeedbackId(id));

        builder.Property(x => x.InterviewScheduleId)
            .HasConversion(x => x.Value, id => new InterviewScheduleId(id))
            .IsRequired();

        builder.Property(x => x.InterviewerEmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .IsRequired();

        builder.Property(x => x.Rating).IsRequired();
        builder.Property(x => x.Strengths).HasMaxLength(2048).IsRequired(false);
        builder.Property(x => x.Concerns).HasMaxLength(2048).IsRequired(false);
        builder.Property(x => x.Recommendation).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => new { x.InterviewScheduleId, x.InterviewerEmployeeId }).IsUnique();

        builder.HasOne(x => x.InterviewSchedule)
            .WithMany(x => x.Feedbacks)
            .HasForeignKey(x => x.InterviewScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Interviewer)
            .WithMany()
            .HasForeignKey(x => x.InterviewerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }

    public void Configure(EntityTypeBuilder<HiringDecision> builder)
    {
        builder.ToTable("HiringDecisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new HiringDecisionId(id));

        builder.Property(x => x.CandidateApplicationId)
            .HasConversion(x => x.Value, id => new CandidateApplicationId(id))
            .IsRequired();

        builder.Property(x => x.Decision).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DecidedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.DecidedAt).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2048).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.CandidateApplicationId).IsUnique();
        builder.HasIndex(x => x.Decision);
        builder.HasIndex(x => x.DecidedAt);
        builder.HasIndex(x => x.DecidedBy);

        builder.HasOne(x => x.CandidateApplication)
            .WithMany()
            .HasForeignKey(x => x.CandidateApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }

    public void Configure(EntityTypeBuilder<RecruitmentRequest> builder)
    {
        builder.ToTable("RecruitmentRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new RecruitmentRequestId(id));
        builder.Property(x => x.RequestNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.RequestedHeadcount).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.Property(x => x.PriorityCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.RequestedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RequestedAt).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ApprovedBy).HasMaxLength(128);
        builder.Property(x => x.RejectedBy).HasMaxLength(128);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.Property(x => x.DepartmentId)
            .HasConversion(x => x.Value, id => new DepartmentId(id))
            .IsRequired();
        builder.Property(x => x.PositionId)
            .HasConversion(x => x.Value, id => new PositionId(id))
            .IsRequired();

        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Histories)
            .WithOne(x => x.RecruitmentRequest)
            .HasForeignKey(x => x.RecruitmentRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.RequestNumber).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.DepartmentId);
        builder.HasIndex(x => x.PositionId);
        builder.HasIndex(x => x.RequestedAt);

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }

    public void Configure(EntityTypeBuilder<RecruitmentRequestHistory> builder)
    {
        builder.ToTable("RecruitmentRequestHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new RecruitmentRequestHistoryId(id));
        builder.Property(x => x.ActionCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OldStatus).HasMaxLength(32);
        builder.Property(x => x.NewStatus).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.PerformedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PerformedAt).IsRequired();

        builder.Property(x => x.RecruitmentRequestId)
            .HasConversion(x => x.Value, id => new RecruitmentRequestId(id))
            .IsRequired();

        builder.HasIndex(x => x.RecruitmentRequestId);
        builder.HasIndex(x => x.PerformedAt);
    }

    public void Configure(EntityTypeBuilder<RecruitmentOpening> builder)
    {
        builder.ToTable("RecruitmentOpenings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new RecruitmentOpeningId(id));
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PlannedHeadcount).IsRequired();
        builder.Property(x => x.FilledHeadcount).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.OpenedAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.Property(x => x.RecruitmentRequestId)
            .HasConversion(x => x.Value, id => new RecruitmentRequestId(id))
            .IsRequired();

        builder.HasOne(x => x.RecruitmentRequest)
            .WithMany()
            .HasForeignKey(x => x.RecruitmentRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RecruitmentRequestId);

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}
