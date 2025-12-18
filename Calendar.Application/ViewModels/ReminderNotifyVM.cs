namespace Calendar.Application.ViewModels;

public sealed class ReminderNotifyVM
{
    public Guid Id { get; set; }
    public DateTime FireAtUtc { get; set; }
    public string EventTitle { get; set; } = "";
    public string? Message { get; set; }

    public int? RepeatEveryMinutes { get; set; }
    public int? RepeatCountLeft { get; set; }
}