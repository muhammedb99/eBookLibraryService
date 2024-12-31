namespace eBookLibraryService.ViewModels
{
    public class ReviewViewModel
    {
        public string UserEmail { get; set; }
        public string Feedback { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
