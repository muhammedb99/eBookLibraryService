using eBookLibraryService.Data;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace eBookLibraryService.Controllers
{
    [Authorize(Roles = "Admin")]  // Only allow access to users with the "Admin" role
    public class DashboardController : Controller
    {
        private readonly eBookLibraryServiceContext _context;

        // Constructor to inject the eBookLibraryServiceContext
        public DashboardController(eBookLibraryServiceContext context)
        {
            _context = context;
        }

        // GET: Dashboard/ManageBooks
        public async Task<IActionResult> ManageBooks()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);  // Return the list of books to the ManageBooks view
        }

        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync(); // Fetch books from the database
            return View(books); // Pass the list of books to the view
        }

        // GET: Dashboard/ManageUsers
        public IActionResult ManageUsers()
        {
            // You can add logic to manage users, e.g., view all users, their status, etc.
            return View();
        }

        // You can add more methods here to handle other admin tasks
    }
}
