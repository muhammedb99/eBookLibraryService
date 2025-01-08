namespace eBookLibraryService.ViewModels
{
    public class WaitingListEntryViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string UserId { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
