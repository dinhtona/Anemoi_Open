using Anemoi.Hr.Domain.Contracts;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class EmployeeContractModelMapping : IEntityTypeConfiguration<EmployeeContract>
{
    public void Configure(EntityTypeBuilder<EmployeeContract> builder)
    {
        builder.ToTable("EmployeeContracts");
        
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeContractId(id));
            
        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ContractNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ContractTypeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1024);
        builder.Property(x => x.AttachmentFileId).HasMaxLength(128);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);

        // Map owned termination details
        builder.OwnsOne(x => x.TerminationDetail, termination =>
        {
            termination.Property(t => t.TerminatedDate).HasColumnName("TerminatedDate");
            termination.Property(t => t.ReasonCode).HasColumnName("TerminationReasonCode").HasMaxLength(64);
            termination.Property(t => t.Notes).HasColumnName("TerminationNotes").HasMaxLength(1024);
            termination.Property(t => t.TerminationAttachmentId).HasColumnName("TerminationAttachmentId").HasMaxLength(128);
        });

        // Indexes
        builder.HasIndex(x => x.ContractNumber).IsUnique();
        builder.HasIndex(x => new { x.EmployeeId, x.StatusCode });
        builder.HasIndex(x => new { x.EmployeeId, x.StartDate, x.EndDate });
        builder.HasIndex(x => new { x.StatusCode, x.EndDate });

        // Relationships
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // PostgreSQL xmin concurrency
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }
}
