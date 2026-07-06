using Anemoi.Hr.Domain.Reporting;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class ReportExportAuditLogModelMapping : IEntityTypeConfiguration<ReportExportAuditLog>
{
    public void Configure(EntityTypeBuilder<ReportExportAuditLog> builder)
    {
        builder.ToTable("ReportExportAuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new ReportExportAuditLogId(id))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.ModuleCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ReportType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ExportedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ExportedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.Format).HasMaxLength(16).IsRequired();
        builder.Property(x => x.TotalRecords).IsRequired();
        builder.Property(x => x.FiltersJson).HasMaxLength(512).IsRequired();
        builder.Property(x => x.FileHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();

        // Index on ExportedAt for time-range queries
        builder.HasIndex(x => x.ExportedAt);

        // Index on ModuleCode + ReportType for module-type filtering
        builder.HasIndex(x => new { x.ModuleCode, x.ReportType });

        // Index on ExportedBy + ExportedAt for user-audit queries
        builder.HasIndex(x => new { x.ExportedBy, x.ExportedAt });
    }
}
