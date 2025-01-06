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
     
    }
}
