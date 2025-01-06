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

        public float? BorrowPrice { get; set; } 

        [Required, Range(0.01, float.MaxValue, ErrorMessage = "Buying price must be greater than 0.")]
        public float BuyingPrice { get; set; }

        [Required]
        public int YearOfPublishing { get; set; }

        [Range(0, 100, ErrorMessage = "Age limitation must be between 0 and 100.")]
        public int? AgeLimitation { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Borrow count must be 0 or greater.")]
        public int BorrowCount { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Purchase count must be 0 or greater.")]
        public int PurchaseCount { get; set; } = 0;

        [Required]
        public string Genre { get; set; }

        public int Popularity => BorrowCount + PurchaseCount;

        public float? DiscountPrice { get; set; }

        public List<int> PublicationYears { get; set; } = new List<int>();
        public List<string> Publishers { get; set; } = new List<string>();

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? DiscountUntil { get; set; }

        public int TotalCopies { get; set; } = 0;
        public int BorrowedCopies { get; set; } = 0; 
        public List<WaitingListEntry> WaitingList { get; set; } = new List<WaitingListEntry>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public DateTime? ReturnDate { get; set; }
        public bool IsReturned => ReturnDate.HasValue;

        [Url(ErrorMessage = "The EPUB link must be a valid URL.")]
        public string PdfLink { get; set; }

        [Url(ErrorMessage = "The EPUB link must be a valid URL.")]
        public string? EpubLink { get; set; }

        [Url(ErrorMessage = "The EPUB link must be a valid URL.")]
        public string? F2bLink { get; set; }

        [Url(ErrorMessage = "The EPUB link must be a valid URL.")]
        public string? MobiLink { get; set; }


    }
}
