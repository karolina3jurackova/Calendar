using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Data.Configuration;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.Title).IsRequired().HasMaxLength(200);

        b.Property(x => x.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.LastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");

        b.HasOne(x => x.Owner)
         .WithMany(u => u.Events)
         .HasForeignKey(x => x.OwnerId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.StartTime);
        b.HasIndex(x => x.EndTime);
        b.HasIndex(nameof(Event.OwnerId), nameof(Event.StartTime));
    }
}