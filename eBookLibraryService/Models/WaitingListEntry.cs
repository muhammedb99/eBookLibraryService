﻿namespace eBookLibraryService.Models
{
    public class WaitingListEntry
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }
        public DateTime DateAdded { get; set; }
        public Book Book { get; set; }
    }
}
