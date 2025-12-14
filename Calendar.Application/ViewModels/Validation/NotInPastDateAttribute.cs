using System;
using System.ComponentModel.DataAnnotations;

namespace Calendar.Application.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotPastDateAttribute : ValidationAttribute
{
    public NotPastDateAttribute()
    {
        ErrorMessage = "Udalosť nemôže začať v minulosti.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true; // Required rieši [Required]

        if (value is not DateTime dt) return false;

        // porovnávame v lokálnom čase (input z datetime-local je lokálny)
        return dt >= DateTime.Now;
    }
}