using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eBookLibraryService.Data;
using eBookLibraryService.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using eBookLibraryService.Models;

namespace eBookLibraryService.Controllers
{
    public class LibraryController : Controller
    {
        private readonly eBookLibraryServiceContext _context;

        public LibraryController(eBookLibraryServiceContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name ?? "user@example.com";

            // Fetch owned books
            var ownedBooks = await _context.OwnedBooks
    .Include(o => o.Book)
    .Where(o => o.UserEmail == userEmail && !o.IsBorrowed)
    .Select(o => new BookDetailsViewModel
    {
        Id = o.Book.Id,
        Title = o.Book.Title,
        Author = o.Book.Author,
        Publisher = o.Book.Publisher,
        BorrowPrice = o.Book.BorrowPrice,
        BuyingPrice = o.Book.BuyingPrice,
        YearOfPublishing = o.Book.YearOfPublishing,
        Genre = o.Book.Genre,
        ImageUrl = o.Book.ImageUrl,
        Reviews = o.Book.Reviews.Select(r => new ReviewViewModel
        {
            UserEmail = r.UserEmail,
            Feedback = r.Feedback,
            Rating = r.Rating
        }).ToList()
    }).ToListAsync();

            // Fetch borrowed books
            var borrowedBooks = await _context.OwnedBooks
                .Include(b => b.Book)
                .ThenInclude(b => b.Reviews)
                .Where(b => b.UserEmail == userEmail && b.IsBorrowed)
                .Select(b => new BookDetailsViewModel 
                {
                    Title = b.Book.Title,
                    Author = b.Book.Author,
                    Publisher = b.Book.Publisher,
                    BorrowPrice = b.Book.BorrowPrice,
                    BuyingPrice = b.Book.BuyingPrice,
                    YearOfPublishing = b.Book.YearOfPublishing,
                    Genre = b.Book.Genre,
                    ImageUrl = b.Book.ImageUrl
                })
                .ToListAsync();

            var model = new LibraryViewModel
            {
                OwnedBooks = ownedBooks,
                BorrowedBooks = borrowedBooks
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AddReview(int bookId, string feedback, int rating)
        {
            // Check if the book exists
            var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
            if (!bookExists)
            {
                TempData["ErrorMessage"] = "The book you are reviewing does not exist.";
                return RedirectToAction("Index"); // Redirect to an appropriate page
            }

            // Create the review
            var review = new Review
            {
                BookId = bookId,
                Feedback = feedback,
                Rating = rating,
                UserEmail = User.Identity.Name ?? "guest@example.com",
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your review has been submitted successfully.";
            return RedirectToAction("Index"); // Redirect back to the library or relevant page
        }

    }
}
