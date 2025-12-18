namespace Calendar.Domain.Entities;

//nevyužívané
public sealed class ShareGroup
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = "";

    public DateTime CreatedUtc { get; set; }

    public ICollection<ShareGroupMember> Members { get; set; } = new List<ShareGroupMember>();
}