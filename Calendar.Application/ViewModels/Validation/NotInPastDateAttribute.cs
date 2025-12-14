using System.ComponentModel.DataAnnotations;

namespace Calendar.Application.Validation;

public sealed class NotInPastDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTime dt)
            return ValidationResult.Success;

        if (dt < DateTime.Now)
            return new ValidationResult(ErrorMessage ?? "Dátum nesmie byť v minulosti.");

        return ValidationResult.Success;
    }
}