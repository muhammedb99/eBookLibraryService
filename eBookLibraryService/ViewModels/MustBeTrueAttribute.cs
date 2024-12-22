using System.ComponentModel.DataAnnotations;

namespace eBookLibraryService.ViewModels // Or replace with the namespace you're using
{
    public class MustBeTrueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is bool booleanValue && booleanValue)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage ?? "You must accept the terms to proceed.");
        }
    }
}
