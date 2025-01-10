using eBookLibraryService.Data;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace eBookLibraryService.Controllers
{
    public class StockController : Controller
    {
        private readonly eBookLibraryServiceContext _context;

        public StockController(eBookLibraryServiceContext context)
        {
            _context = context;
        }

        // GET: Stock/Index
        public async Task<IActionResult> Index()
        {
            // Use eager loading to include the WaitingList property
            var books = await _context.Books
                .Include(b => b.WaitingList) // Include the WaitingList property
                .ToListAsync();

            return View(books);
        }
    }
}
