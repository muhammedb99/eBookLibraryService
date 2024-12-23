using eBookLibraryService.Data;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace eBookLibraryService.Controllers
{
    public class ReportsController : Controller
    {
        private readonly eBookLibraryServiceContext _context;

        public ReportsController(eBookLibraryServiceContext context)
        {
            _context = context;
        }

        // GET: Reports/TopSellingItems
        public async Task<IActionResult> TopSellingItems()
        {
            // Query the database to get the top-selling books based on BorrowCount + PurchaseCount
            var topSellingItems = await _context.Books
                .OrderByDescending(b => b.BorrowCount + b.PurchaseCount)
                .Take(10) // Get top 10 selling books
                .ToListAsync();

            return View(topSellingItems); // Pass the top-selling books to the view
        }
    }
}
