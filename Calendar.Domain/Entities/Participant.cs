namespace Calendar.Domain.Entities;

public class Participant
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; } = default!;

    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? Nickname { get; set; }
}