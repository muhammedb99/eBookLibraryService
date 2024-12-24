using System;
using System.Collections.Generic;
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

        // Prices set as float for borrowing and buying
        public float? BorrowPrice { get; set; } // Nullable for books that can't be borrowed

        [Required, Range(0.01, float.MaxValue, ErrorMessage = "Buying price must be greater than 0.")]
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

        // Computed property for popularity based on borrowing and purchase counts
        public int Popularity => BorrowCount + PurchaseCount;

        // Optional discount price for promotional offers
        public float? DiscountPrice { get; set; }

        // Allow tracking of past publication years and publishers for a book
        public List<int> PublicationYears { get; set; } = new List<int>();
        public List<string> Publishers { get; set; } = new List<string>();

        // Metadata for book creation and discount periods
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? DiscountUntil { get; set; }

        // Borrowing and waiting list tracking
        public int TotalCopies { get; set; } = 0;
        public int BorrowedCopies { get; set; } = 0; // Current borrowed count
        public List<WaitingListEntry> WaitingList { get; set; } = new List<WaitingListEntry>();

        // Borrowing return logic
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned => ReturnDate.HasValue;
    }
}
