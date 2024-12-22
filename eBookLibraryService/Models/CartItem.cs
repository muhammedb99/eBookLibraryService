namespace eBookLibraryService.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public Book Book { get; set; }
        public bool IsBorrow { get; set; }
        public float Price { get; set; }
    }
}
