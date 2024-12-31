namespace eBookLibraryService.ViewModels
{
    public class BookDetailsWithReviewViewModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public float? BorrowPrice { get; set; }
        public float BuyingPrice { get; set; }
        public int YearOfPublishing { get; set; }
        public string AgeLimitation { get; set; }
        public int Quantity { get; set; }
        public string Genre { get; set; }
        public string ImageUrl { get; set; }
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
    }
}
