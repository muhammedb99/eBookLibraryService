using eBookLibraryService.Data;
using eBookLibraryService.Models;
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

        // GET: Dashboard/ManageUsers
        public IActionResult ManageUsers()
        { 
            return RedirectToAction("ManageUsers", "Account");
        }
    }
}
