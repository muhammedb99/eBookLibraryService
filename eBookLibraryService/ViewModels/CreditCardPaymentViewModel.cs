using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace eBookLibraryService.ViewModels
{
    public class CreditCardPaymentViewModel : IValidatableObject
    {
        public float TotalAmount { get; set; }

        [Required(ErrorMessage = "Card number is required.")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Card number must be 16 digits.")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiration date is required.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/(\d{2})$", ErrorMessage = "Expiration date must be in MM/YY format.")]
        public string ExpirationDate { get; set; }

        [Required(ErrorMessage = "CVV is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV must be 3 digits.")]
        public string CVV { get; set; }

        [Required(ErrorMessage = "Cardholder name is required.")]
        [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Cardholder name must contain only letters and spaces.")]
        public string CardHolderName { get; set; }
        public int? BookId { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsExpirationDateValid())
            {
                yield return new ValidationResult("Invalid expiration date. It must be in the future and less than 5 years from now.",
                    new[] { nameof(ExpirationDate) });
            }
        }

        private bool IsExpirationDateValid()
        {
            if (string.IsNullOrWhiteSpace(ExpirationDate))
                return false;

            var parts = ExpirationDate.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
                return false;

            if (month < 1 || month > 12)
                return false;

            var today = DateTime.Now;
            var currentYear = today.Year % 100;
            var currentMonth = today.Month;
            var maxYear = currentYear + 5;

            return year > currentYear || (year == currentYear && month >= currentMonth) && year <= maxYear;
        }
    }
}
