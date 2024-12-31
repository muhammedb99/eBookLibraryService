using eBookLibraryService.Models;
using System.Collections.Generic;

namespace eBookLibraryService.ViewModels
{
    public class LibraryViewModel
    {
        public List<LibrarySectionsViewModel> Sections { get; set; }
        public List<BookDetailsViewModel> OwnedBooks { get; set; } = new List<BookDetailsViewModel>();
        public List<BookDetailsViewModel> BorrowedBooks { get; set; } = new List<BookDetailsViewModel>();
    }

}
