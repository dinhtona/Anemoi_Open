using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class PayslipDocumentModelMapping :
    IEntityTypeConfiguration<PayslipDocument>,
    IEntityTypeConfiguration<PayslipEmailDelivery>
{
    public void Configure(EntityTypeBuilder<PayslipDocument> builder)
    {
        builder.ToTable("PayslipDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new PayslipDocumentId(id));

        builder.Property(x => x.PayslipId)
            .HasConversion(x => x.Value, id => new PayslipId(id))
            .IsRequired();

        builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.ChecksumHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.GeneratedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.GeneratedAt).IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Indexes
        builder.HasIndex(x => x.PayslipId);
        builder.HasIndex(x => new { x.PayslipId, x.IsActive });

        // Relationships
        builder.HasOne(x => x.Payslip)
            .WithMany()
            .HasForeignKey(x => x.PayslipId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<PayslipEmailDelivery> builder)
    {
        builder.ToTable("PayslipEmailDeliveries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new PayslipEmailDeliveryId(id));

        builder.Property(x => x.PayslipId)
            .HasConversion(x => x.Value, id => new PayslipId(id))
            .IsRequired();

        builder.Property(x => x.PayslipDocumentId)
            .HasConversion(x => x.Value, id => new PayslipDocumentId(id))
            .IsRequired();

        builder.Property(x => x.ToEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(256).IsRequired();
        
        builder.Property(x => x.Status)
            .HasConversion(s => s.ToString(), v => (PayslipEmailDeliveryStatus)System.Enum.Parse(typeof(PayslipEmailDeliveryStatus), v))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ErrorMessage).HasMaxLength(2048).IsRequired(false);
        builder.Property(x => x.SentBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SentAt).IsRequired(false);
        builder.Property(x => x.FailedAt).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Indexes
        builder.HasIndex(x => x.PayslipId);
        builder.HasIndex(x => x.PayslipDocumentId);

        // Relationships
        builder.HasOne(x => x.Payslip)
            .WithMany()
            .HasForeignKey(x => x.PayslipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PayslipDocument)
            .WithMany()
            .HasForeignKey(x => x.PayslipDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
