using System.Collections.Generic;
using System.Text.Json;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Notification.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Notification.Infrastructure.DataContext;

public sealed class ModelMapping :
    IEntityTypeConfiguration<NotificationHistory>,
    IEntityTypeConfiguration<NotificationSubscription>
{
    public void Configure(EntityTypeBuilder<NotificationHistory> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new NotificationHistoryId(id));
        builder.HasKey(x => x.Id);
        
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.WorkspaceId);
        builder.HasIndex(x => x.CreatedTime);

        builder.Property(x => x.TitleLocalizationArgs)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
            .HasColumnType("text");

        builder.Property(x => x.ContentLocalizationArgs)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
            .HasColumnType("text");

        builder.HasIndex(x => new { x.UserId, x.DeduplicationKey })
            .IsUnique()
            .HasFilter("\"DeduplicationKey\" IS NOT NULL");
    }

    public void Configure(EntityTypeBuilder<NotificationSubscription> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new NotificationSubscriptionId(id));
        builder.HasKey(x => x.Id);
        
        builder.HasIndex(x => new { x.UserId, x.Category }).IsUnique();
    }
}
