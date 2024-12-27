using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eBookLibraryService.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Cart")]
        public int CartId { get; set; } 
        public Cart Cart { get; set; } 

        [Required]
        [ForeignKey("Book")]
        public int BookId { get; set; } 
        public Book Book { get; set; } 

        [Required]
        public bool IsBorrow { get; set; } 

        [Required]
        [Range(0, float.MaxValue, ErrorMessage = "Price must be 0 or greater.")]
        public float Price { get; set; } 

        public bool IsConfirmed { get; set; } 
    }
}
