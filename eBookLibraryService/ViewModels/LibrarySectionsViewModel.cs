namespace eBookLibraryService.ViewModels
{
    public class LibrarySectionsViewModel
    {
        public string SectionTitle { get; set; } // Title of the section (e.g., Owned Books, Borrowed Books)
        public List<BookDetailsViewModel> Books { get; set; } // List of books in this section
    }
}
