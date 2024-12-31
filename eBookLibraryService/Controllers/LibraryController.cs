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
            // Ensure the user is logged in and fetch their email
            var userEmail = User.Identity?.Name ?? "user@example.com";

            // Fetch owned books including their details
            var ownedBooks = await _context.OwnedBooks
                .Include(o => o.Book)
                .Where(o => o.UserEmail == userEmail)
                .Select(o => new BookDetailsViewModel
                {
                    Cover = o.Book.ImageUrl,
                    Title = o.Book.Title,
                    Author = o.Book.Author,
                    PublishYear = o.Book.PublicationYears.FirstOrDefault(),
                    Publisher = o.Book.Publisher,
                    IsBorrowed = false 
                })
                .ToListAsync();

            // Fetch borrowed books including their details
            var borrowedBooks = await _context.BorrowedBooks
                .Include(b => b.Book)
                .Where(b => b.UserEmail == userEmail)
                .Select(b => new BookDetailsViewModel
                {
                    Cover = b.Book.ImageUrl,
                    Title = b.Book.Title,
                    Author = b.Book.Author,
                    PublishYear = b.Book.PublicationYears.FirstOrDefault(),
                    Publisher = b.Book.Publisher,
                    IsBorrowed = true 
                })
                .ToListAsync();

            // Combine both owned and borrowed books
            var libraryBooks = ownedBooks.Concat(borrowedBooks).ToList();

            // Pass the combined list to the view
            return View(libraryBooks);
        }
    }
}
