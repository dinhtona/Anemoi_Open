using Anemoi.Hr.Domain.EmployeeNotes;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class EmployeeNoteModelMapping : IEntityTypeConfiguration<EmployeeNote>
{
    public void Configure(EntityTypeBuilder<EmployeeNote> builder)
    {
        builder.ToTable("EmployeeNotes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeNoteId(id));

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));

        builder.Property(x => x.Content).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.NoteCategory)
            .HasConversion(c => c.Value, v => NoteCategory.FromValue(v))
            .HasMaxLength(50);

        builder.Property(x => x.CreatedByUserId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.EmployeeId);
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}
