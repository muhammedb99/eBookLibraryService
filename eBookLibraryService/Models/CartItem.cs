using System.ComponentModel.DataAnnotations;

namespace eBookLibraryService.Models
{
    public class CartItem
    {
        public int Id { get; set; } 

        [Required]
        public Book Book { get; set; } 

        [Required]
        public bool IsBorrow { get; set; } 

        [Required]
        [Range(0, float.MaxValue, ErrorMessage = "Price must be 0 or greater.")]
        public float Price { get; set; } 

        public bool IsConfirmed { get; set; } 
    }
}
