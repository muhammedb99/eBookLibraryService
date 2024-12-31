using eBookLibraryService.Models;
using System.Collections.Generic;

namespace eBookLibraryService.ViewModels
{
    public class LibraryViewModel
    {
        public List<BorrowedBook> BorrowedBooks { get; set; }
        public List<OwnedBook> OwnedBooks { get; set; }
    }
}
