using eBookLibraryService.Data;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        // POST: Stock/UpdateStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(Dictionary<int, int> quantities)
        {
            // Loop through each book ID and update the quantity
            foreach (var kvp in quantities)
            {
                var bookId = kvp.Key;
                var newQuantity = kvp.Value;

                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
                if (book != null)
                {
                    book.Quantity = newQuantity;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
