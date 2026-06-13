using Anemoi.Hr.Domain.Ess;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class EssModelMapping : IEntityTypeConfiguration<EmployeePortalAccess>
{
    public void Configure(EntityTypeBuilder<EmployeePortalAccess> builder)
    {
        builder.ToTable("EmployeePortalAccesses");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeePortalAccessId(id));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id))
            .IsRequired();

        builder.HasIndex(x => x.EmployeeId).IsUnique();

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();
    }
}
