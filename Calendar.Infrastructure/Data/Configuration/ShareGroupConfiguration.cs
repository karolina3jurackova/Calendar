using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Data.Configuration;

public sealed class ShareGroupConfiguration : IEntityTypeConfiguration<ShareGroup>
{
    public void Configure(EntityTypeBuilder<ShareGroup> b)
    {
        b.ToTable("ShareGroups");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(64).IsRequired();
        b.Property(x => x.CreatedUtc).IsRequired();

        b.HasIndex(x => new { x.OwnerId, x.Name }).IsUnique();
    }
}

public sealed class ShareGroupMemberConfiguration : IEntityTypeConfiguration<ShareGroupMember>
{
    public void Configure(EntityTypeBuilder<ShareGroupMember> b)
    {
        b.ToTable("ShareGroupMembers");
        b.HasKey(x => x.Id);

        b.Property(x => x.AddedUtc).IsRequired();

        b.HasOne(x => x.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.GroupId, x.UserId }).IsUnique();
    }
}