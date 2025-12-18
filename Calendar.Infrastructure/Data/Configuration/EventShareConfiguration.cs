using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Data.Configuration;

public sealed class EventShareConfiguration : IEntityTypeConfiguration<EventShare>
{
    public void Configure(EntityTypeBuilder<EventShare> b)
    {
        b.ToTable("EventShares");

        b.HasKey(x => x.Id);

        b.Property(x => x.Access).IsRequired();
        b.Property(x => x.CreatedUtc).IsRequired();

        b.HasOne(x => x.Event)
            .WithMany(e => e.EventShares)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // nevyužívané
        b.HasIndex(x => new { x.EventId, x.SharedWithUserId })
            .IsUnique()
            .HasFilter("SharedWithUserId IS NOT NULL");

        b.HasIndex(x => new { x.EventId, x.SharedWithGroupId })
            .IsUnique()
            .HasFilter("SharedWithGroupId IS NOT NULL");
    }
}