using System;

namespace eBookLibraryService.Models
{
    public class BorrowedBook
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book Book{ get; set; }
        public string UserEmail { get; set; }
        public DateTime BorrowedDate { get; set; }
        public DateTime? ReturnDate { get; set; } 
        public bool IsReturned { get; set; }

    }

}
