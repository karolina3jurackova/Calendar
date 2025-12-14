using System.ComponentModel.DataAnnotations;
using Calendar.Application.Validation;

namespace Calendar.Application.ViewModels;

// =======================
// LIST
// =======================
public sealed class EventListItemVM
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

// =======================
// DETAIL
// =======================
public sealed class EventDetailVM
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

// =======================
// CREATE / EDIT BASE
// =======================
public class EventCreateVM : IValidatableObject
{
    [Required, StringLength(100)]
    public string Title { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [NotInPastDate(ErrorMessage = "Udalosť nemôže začínať v minulosti.")]
    public DateTime Start { get; set; }

    [Required]
    public DateTime End { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (End <= Start)
        {
            yield return new ValidationResult(
                "Koniec musí byť po začiatku.",
                new[] { nameof(Start), nameof(End) }
            );
        }
    }
}

// =======================
// EDIT = CREATE
// =======================
public sealed class EventEditVM : EventCreateVM
{
}