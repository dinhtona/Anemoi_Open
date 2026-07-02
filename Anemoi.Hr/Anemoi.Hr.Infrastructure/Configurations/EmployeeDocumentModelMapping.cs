using Anemoi.Hr.Domain.EmployeeDocuments;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class EmployeeDocumentModelMapping : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("EmployeeDocuments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeDocumentId(id));

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));

        builder.Property(x => x.DocumentType)
            .HasConversion(t => t.Value, v => DocumentType.FromValue(v))
            .HasMaxLength(50).IsRequired();

        builder.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.IssuedBy).HasMaxLength(200);
        builder.Property(x => x.StorageKey).HasMaxLength(500);
        builder.Property(x => x.FileName).HasMaxLength(256);
        builder.Property(x => x.MimeType).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.EmployeeId);
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}
