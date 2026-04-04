using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.MasterData.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.MasterData.Infrastructure.DataContext;

public sealed class ModelMapping :
    IEntityTypeConfiguration<Province>,
    IEntityTypeConfiguration<District>,
    IEntityTypeConfiguration<SeedServer>,
    IEntityTypeConfiguration<SeedFunction>,
    IEntityTypeConfiguration<SeedTemplate>

{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new ProvinceId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(512);
        builder.Property(x => x.Slug)
            .HasMaxLength(512);
        builder.Property(x => x.SearchHint)
            .HasMaxLength(512);
        builder.HasIndex(x => x.Slug);
    }

    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new DistrictId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(512);
        builder.HasOne(x => x.Province)
            .WithMany(x => x.Districts)
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Property(x => x.Slug)
            .HasMaxLength(512);
        builder.Property(x => x.SearchHint)
            .HasMaxLength(512);
        builder.HasIndex(x => x.Slug);
    }

    public void Configure(EntityTypeBuilder<SeedServer> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new SeedServerId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(512);
        builder.Property(x => x.ConnectionString)
            .IsRequired();
        builder.Property(x => x.Environment)
            .HasMaxLength(256);
        builder.HasIndex(x => x.Name)
            .IsUnique();
    }

    public void Configure(EntityTypeBuilder<SeedFunction> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new SeedFunctionId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(512);
        builder.HasOne(x => x.SeedServer)
            .WithMany(x => x.SeedFunctions)
            .HasForeignKey(x => x.SeedServerId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.SeedTemplate)
            .WithMany(x => x.RelatedFunctions)
            .HasForeignKey(x => x.SeedTemplateId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.Name)
            .IsUnique();
    }

    public void Configure(EntityTypeBuilder<SeedTemplate> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new SeedTemplateId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(512);
        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}