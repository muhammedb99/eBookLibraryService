namespace eBookLibraryService.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public Book Book { get; set; }
        public bool IsBorrow { get; set; } // True if it's a borrow, false if it's a purchase
    }
}
