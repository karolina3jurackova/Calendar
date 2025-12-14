using Calendar.Domain.Enums;

namespace Calendar.Domain.Entities;

public class Reminder
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; } = default!;

    public ReminderType Type { get; set; }   // Relative/Absolute
    public int? MinutesBefore { get; set; }  // pri Relative
    public DateTime? AtUtc { get; set; }     // pri Absolute

    public bool ChannelLocal { get; set; } = true;
    public bool ChannelEmail { get; set; } = false;
}