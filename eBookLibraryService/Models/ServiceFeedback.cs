namespace eBookLibraryService.Models
{
    public class ServiceFeedback
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; } 
        public string Feedback { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
