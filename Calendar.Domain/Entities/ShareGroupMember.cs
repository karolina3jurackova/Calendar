namespace Calendar.Domain.Entities;

public sealed class ShareGroupMember
{
    public Guid Id { get; set; }

    public Guid GroupId { get; set; }
    public ShareGroup Group { get; set; } = null!;

    public Guid UserId { get; set; }
    public DateTime AddedUtc { get; set; }
}