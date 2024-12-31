namespace eBookLibraryService.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public string UserEmail { get; set; } 
        public string Feedback { get; set; } 
        public int Rating { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
