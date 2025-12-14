using Calendar.Domain.Enums;

namespace Calendar.Domain.Entities;

public class Share
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; } = default!;

    public ShareGroup GroupKey { get; set; }
    public DateTime DateCreated { get; set; }
}