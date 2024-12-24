using System;

namespace eBookLibraryService.Models
{
    public class WaitingListEntry
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string UserId { get; set; } // Add this field for tracking the user
    public DateTime DateAdded { get; set; }
}

}
