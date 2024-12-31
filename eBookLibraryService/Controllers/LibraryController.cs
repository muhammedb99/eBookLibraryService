using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eBookLibraryService.Data;
using eBookLibraryService.ViewModels;
using System.Linq;
using System.Threading.Tasks;

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
                    Cover = o.Book.ImageUrl,
                    Title = o.Book.Title,
                    Author = o.Book.Author,
                    PublishYear = o.Book.YearOfPublishing,
                    Publisher = o.Book.Publisher
                })
                .ToListAsync();

            // Fetch borrowed books
            var borrowedBooks = await _context.OwnedBooks
                .Include(b => b.Book)
                .Where(b => b.UserEmail == userEmail && b.IsBorrowed)
                .Select(b => new BookDetailsViewModel
                {
                    Cover = b.Book.ImageUrl,
                    Title = b.Book.Title,
                    Author = b.Book.Author,
                    PublishYear = b.Book.YearOfPublishing,
                    Publisher = b.Book.Publisher
                })
                .ToListAsync();

            var model = new LibraryViewModel
            {
                OwnedBooks = ownedBooks,
                BorrowedBooks = borrowedBooks
            };

            return View(model);
        }


    }
}
