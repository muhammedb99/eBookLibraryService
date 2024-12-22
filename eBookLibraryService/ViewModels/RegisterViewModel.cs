using System.ComponentModel.DataAnnotations;

namespace eBookLibraryService.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 40 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "phone number must be 10 numbers ")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [MustBeTrue(ErrorMessage = "You must accept the terms to register.")]
        [Display(Name = "Accept Terms")]
        public bool AcceptTerms { get; set; }
    }
}
