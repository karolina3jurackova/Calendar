namespace Calendar.Domain.Entities;

public enum ReminderType
{
    RelativeMinutesBefore = 1, // napr. 10 min pred
    AbsoluteUtc = 2            // konkrétny UTC čas
}

public enum ReminderChannel
{
    Browser = 1,
    Email = 2
}

public class Reminder
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public ReminderType Type { get; set; }
    public ReminderChannel Channel { get; set; }

    public int? MinutesBefore { get; set; }
    public DateTime? AbsoluteUtc { get; set; }

    public int? RepeatEveryMinutes { get; set; }
    public int? RepeatCountLeft { get; set; }

    public DateTime FireAtUtc { get; set; }
    public bool IsSent { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}