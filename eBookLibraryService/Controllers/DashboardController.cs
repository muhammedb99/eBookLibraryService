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
        private readonly AppDbContext _appDbContext;
        private readonly eBookLibraryServiceContext _context;

        // Constructor to inject the eBookLibraryServiceContext
        public DashboardController(eBookLibraryServiceContext context, AppDbContext appDbContext)
        {
            _context = context;
            _appDbContext = appDbContext;
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
            // Retrieve list of users (implement this based on your application logic)
            var users = _appDbContext.Users.ToList(); // Assuming you have a `Users` table in your DB
            return View(users); // Pass users data to the view
        }

        // You can add more methods here to handle other admin tasks
    }
}
