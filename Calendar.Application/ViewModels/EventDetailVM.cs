namespace Calendar.Application.ViewModels;

public sealed class EventDetailVM
{
    public Guid Id { get; set; }

    public string Title { get; set; } = "";
    public string? Description { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public string? Location { get; set; }
    public bool AllDay { get; set; }

    public IList<ReminderItemVM> Reminders { get; set; } = new List<ReminderItemVM>();
}