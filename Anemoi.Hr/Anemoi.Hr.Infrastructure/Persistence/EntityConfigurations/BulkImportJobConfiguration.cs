using Anemoi.Hr.Domain.BulkImport;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Persistence.EntityConfigurations;

public sealed class BulkImportJobConfiguration : IEntityTypeConfiguration<BulkImportJob>
{
    public void Configure(EntityTypeBuilder<BulkImportJob> builder)
    {
        builder.ToTable("bulk_import_jobs", "hr");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, v => new BulkImportJobId(v))
            .ValueGeneratedNever();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ActorUserId).HasMaxLength(100);
        builder.Property(x => x.ActorName).HasMaxLength(255);
        builder.Property(x => x.PreviewDataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ErrorDetails).HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.StatusCode);
    }
}
