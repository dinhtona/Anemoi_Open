using Anemoi.Hr.Domain.EmployeeAssets;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class EmployeeAssetModelMapping : IEntityTypeConfiguration<EmployeeAsset>
{
    public void Configure(EntityTypeBuilder<EmployeeAsset> builder)
    {
        builder.ToTable("EmployeeAssets");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeAssetId(id));

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));

        builder.Property(x => x.AssetType)
            .HasConversion(t => t.Value, v => AssetType.FromValue(v))
            .HasMaxLength(50).IsRequired();

        builder.Property(x => x.AssetTag).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Brand).HasMaxLength(100);
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.Property(x => x.SerialNumber).HasMaxLength(100);
        builder.Property(x => x.AssetStatus)
            .HasConversion(s => s.Value, v => AssetStatus.FromValue(v))
            .HasMaxLength(50).IsRequired();

        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.AssetTag).IsUnique();
        builder.HasIndex(x => x.EmployeeId);
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}
