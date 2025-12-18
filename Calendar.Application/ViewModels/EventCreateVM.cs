using System.ComponentModel.DataAnnotations;
using Calendar.Application.Validation;

namespace Calendar.Application.ViewModels;

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

    public string? ShareWithEmails { get; set; }

    // VALIDATION
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (End <= Start)
        {
            yield return new ValidationResult(
                "Koniec musí byť po začiatku.",
                new[] { nameof(End) }
            );
        }
    }
}