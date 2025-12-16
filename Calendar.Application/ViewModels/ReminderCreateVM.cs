using System.ComponentModel.DataAnnotations;
using Calendar.Domain.Entities;

namespace Calendar.Application.ViewModels;

public sealed class ReminderCreateVM
{
    [Required]
    public ReminderType Type { get; set; }

    [Required]
    public ReminderChannel Channel { get; set; }

    // RelativeMinutesBefore
    public int? MinutesBefore { get; set; }

    // Absolute – zadávaš lokálne (service prekonvertuje do UTC)
    public DateTime? AbsoluteLocal { get; set; }

    // opakovanie (voliteľné)
    public int? RepeatEveryMinutes { get; set; }
    public int? RepeatCountLeft { get; set; }
}