using System.ComponentModel.DataAnnotations;

namespace eBookLibraryService.ViewModels
{
    public class CreditCardPaymentViewModel
    {
        public float TotalAmount { get; set; }

        [Required(ErrorMessage = "Card number is required.")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Card number must be 16 digits.")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiration date is required.")]
        public string ExpirationDate { get; set; } 

        [Required(ErrorMessage = "CVV is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV must be 3 digits.")]
        public string CVV { get; set; }

        [Required(ErrorMessage = "Cardholder name is required.")]
        public string CardHolderName { get; set; }
    }
}
