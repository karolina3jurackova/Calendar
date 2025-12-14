using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Data.Configuration;

public class ShareConfiguration : IEntityTypeConfiguration<Share>
{
    public void Configure(EntityTypeBuilder<Share> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Event)
         .WithMany(e => e.Shares)
         .HasForeignKey(x => x.EventId)
         .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.HasIndex(x => new { x.EventId, x.GroupKey }).IsUnique();
    }
}