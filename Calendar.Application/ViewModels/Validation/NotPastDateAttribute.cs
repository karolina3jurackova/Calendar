using System.ComponentModel.DataAnnotations;

namespace Calendar.Application.ViewModels.Validation;

public sealed class NotPastDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return ValidationResult.Success;

        if (value is DateTime dt)
        {
            // berieme to ako "local"
            var local = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            if (local < DateTime.Now)
                return new ValidationResult(ErrorMessage ?? "Dátum nesmie byť v minulosti.");
        }

        return ValidationResult.Success;
    }
}