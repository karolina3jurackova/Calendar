namespace Calendar.Domain.Entities;

public enum EventShareAccess
{
    Read = 0,
    Edit = 1
}

public sealed class EventShare
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public Guid? SharedWithUserId { get; set; }
    public EventShareAccess Access { get; set; } = EventShareAccess.Read;
    public DateTime CreatedUtc { get; set; }
    public Guid? SharedWithGroupId { get; set; } //nevyužívané, pripravené na rozšírenie
}