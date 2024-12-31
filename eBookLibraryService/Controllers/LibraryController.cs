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
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "You need to be logged in to access your library.";
                return RedirectToAction("Login", "Account");
            }

            var ownedBooks = await _context.OwnedBooks
                .Include(o => o.Book)
                .Where(o => o.UserEmail == userEmail)
                .Select(o => new BookDetailsViewModel
                {
                    Cover = o.Book.ImageUrl ?? "default_cover.jpg", 
                    Title = o.Book.Title,
                    Author = o.Book.Author,
                    PublishYear = o.Book.YearOfPublishing, 
                    Publisher = o.Book.Publisher,
                    IsBorrowed = false 
                })
                .ToListAsync();

        
            var borrowedBooks = await _context.BorrowedBooks
                .Include(b => b.Book)
                .Where(b => b.UserEmail == userEmail)
                .Select(b => new BookDetailsViewModel
                {
                    Cover = b.Book.ImageUrl ?? "default_cover.jpg", 
                    Title = b.Book.Title,
                    Author = b.Book.Author,
                    PublishYear = b.Book.YearOfPublishing,
                    Publisher = b.Book.Publisher,
                    IsBorrowed = true 
                })
                .ToListAsync();

            var libraryBooks = ownedBooks.Concat(borrowedBooks).ToList();

            return View(libraryBooks);
        }
    }
}
