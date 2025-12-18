using System.ComponentModel.DataAnnotations;
using Calendar.Domain.Entities;

namespace Calendar.Application.ViewModels;

public sealed class ReminderCreateVM
{
    [Required]
    public ReminderType Type { get; set; }

    [Required]
    public ReminderChannel Channel { get; set; }

    public int? MinutesBefore { get; set; }

    public DateTime? AbsoluteLocal { get; set; }
    public int? RepeatEveryMinutes { get; set; }
    public int? RepeatCountLeft { get; set; }
}