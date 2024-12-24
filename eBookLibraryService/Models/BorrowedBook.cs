using System;

namespace eBookLibraryService.Models
{
    public class BorrowedBook
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string Email { get; set; } // User reference
        public DateTime BorrowedDate { get; set; }
        public DateTime? ReturnDate { get; set; } // Nullable ReturnDate
        public bool IsReturned { get; set; }
    }

}
