namespace Calendar.Application.ViewModels;

public sealed class EventCalendarVM
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public List<ReminderItemVM> Reminders { get; set; } = new();
    public DateTime Start { get; set; }   // lokálny čas (už pre UI)
    public DateTime End { get; set; }     // lokálny čas (už pre UI)
}