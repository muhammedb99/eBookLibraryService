using Microsoft.AspNetCore.Mvc;
using eBookLibraryService.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using eBookLibraryService.ViewModels;

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
            var userEmail = User.Identity.Name ?? "user@example.com";

            var ownedBooks = await _context.OwnedBooks
                .Include(o => o.Book)
                .Where(o => o.UserEmail == userEmail)
                .ToListAsync();

            var borrowedBooks = await _context.BorrowedBooks
                .Include(b => b.Book)
                .Where(b => b.UserEmail == userEmail)
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
