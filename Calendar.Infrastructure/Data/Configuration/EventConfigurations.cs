using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Data.Configuration;

public class EventConfigurations : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        // PK
        builder.HasKey(e => e.Id);

        // Title
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        // Description
        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        // Time
        builder.Property(e => e.StartTime)
            .IsRequired();

        builder.Property(e => e.EndTime)
            .IsRequired();

        // Owner (Identity user ID – bez navigácie)
        builder.Property(e => e.OwnerId)
            .IsRequired();

        // Index pre "moje eventy"
        builder.HasIndex(e => e.OwnerId);

        // Auditing
        builder.Property(e => e.DateCreated)
            .IsRequired();

        builder.Property(e => e.LastModified)
            .IsRequired();
    }
}