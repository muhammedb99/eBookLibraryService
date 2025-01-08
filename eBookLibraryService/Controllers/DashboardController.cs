using eBookLibraryService.Data;
using eBookLibraryService.Models;
using eBookLibraryService.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace eBookLibraryService.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class DashboardController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly eBookLibraryServiceContext _context;

        public DashboardController(eBookLibraryServiceContext context, AppDbContext appDbContext)
        {
            _context = context;
            _appDbContext = appDbContext;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageWaitingList()
        {
            var waitingListEntries = await _context.WaitingListEntries
                .Include(w => w.Book) 
                .Select(w => new WaitingListEntryViewModel
                {
                    Id = w.Id,
                    BookId = w.BookId,
                    BookTitle = w.Book.Title,
                    UserId = w.UserId,
                    DateAdded = w.DateAdded
                })
                .ToListAsync();

            return View(waitingListEntries);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWaitingList(int id)
        {
            var entry = await _context.WaitingListEntries.FindAsync(id);
            if (entry != null)
            {
                _context.WaitingListEntries.Remove(entry);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Entry removed from the waiting list.";
            }
            else
            {
                TempData["Error"] = "Entry not found.";
            }

            return RedirectToAction("ManageWaitingList");
        }


        public async Task<IActionResult> ManageBooks()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);  
        }

        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync(); 
            return View(books); 
        }

        public IActionResult ManageUsers()
        { 
            return RedirectToAction("ManageUsers", "Account");
        }
    }
}
