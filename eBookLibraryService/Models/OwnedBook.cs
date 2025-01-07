using System;
using System.ComponentModel.DataAnnotations;

namespace eBookLibraryService.Models
{
    public class OwnedBook
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } 
        public Users User { get; set; }       

        [Required]
        public int BookId { get; set; }       
        public Book Book { get; set; }       

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public bool IsBorrowed { get; set; }

        [Required]
        public float Price { get; set; }

        public DateTime PurchaseDate { get; set; }
    }
}
