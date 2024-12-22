using System.ComponentModel.DataAnnotations;

namespace eBookLibraryService.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        public string Publisher { get; set; }

        public float? BorrowPrice { get; set; } // Make BorrowPrice nullable

        [Required, DataType(DataType.Currency)]
        public float BuyingPrice { get; set; }

        [Required]
        public int YearOfPublishing { get; set; }

        [Range(0, 100, ErrorMessage = "Age limitation must be between 0 and 100.")]
        public int? AgeLimitation { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Borrow count must be 0 or greater.")]
        public int BorrowCount { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Purchase count must be 0 or greater.")]
        public int PurchaseCount { get; set; } = 0;

        [Required]
        public string Genre { get; set; }

        public int Popularity => BorrowCount + PurchaseCount;
    }
}
