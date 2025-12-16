using Calendar.Domain.Entities;

namespace Calendar.Application.ViewModels;

public sealed class ReminderItemVM
{
    public Guid Id { get; set; }

    public ReminderType Type { get; set; }
    public ReminderChannel Channel { get; set; }

    public int? MinutesBefore { get; set; }

    // lokálny čas (service konvertuje z FireAtUtc)
    public DateTime FireAtLocal { get; set; }

    public bool IsSent { get; set; }
}