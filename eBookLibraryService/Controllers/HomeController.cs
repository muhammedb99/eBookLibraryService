using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace eBookLibraryService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly eBookLibraryServiceContext _context;

        public HomeController(ILogger<HomeController> logger, eBookLibraryServiceContext context)
        {
            _logger = logger;
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Retrieve the cart from session and set the cart item count in ViewBag
            var cart = HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
            ViewBag.CartItemCount = cart.Items.Count;
            base.OnActionExecuting(context);
        }

        // Index action - Fetching books with optional sorting
        public async Task<IActionResult> Index(string sortOrder = null)
        {
            var books = _context.Books.AsQueryable(); // Fetch books as a queryable object

            // Apply sorting based on the sortOrder parameter
            books = sortOrder switch
            {
                "price_asc" => books.OrderBy(b => b.BuyingPrice),
                "price_desc" => books.OrderByDescending(b => b.BuyingPrice),
                "popular" => books.OrderByDescending(b => b.Popularity), // Assuming 'Popularity' is a property
                "genre" => books.OrderBy(b => b.Genre),
                "year" => books.OrderByDescending(b => b.YearOfPublishing),
                _ => books
            };

            return View(await books.ToListAsync()); // Return sorted books to the view
        }

        // Search action - Fetching books based on search query with optional sorting
        public IActionResult Search(string query, string sortOrder = null)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                // Filter books based on the query
                books = books.Where(b => b.Title.Contains(query) || b.Author.Contains(query));
            }

            // Apply sorting based on the sortOrder parameter
            books = sortOrder switch
            {
                "price_asc" => books.OrderBy(b => b.BuyingPrice),
                "price_desc" => books.OrderByDescending(b => b.BuyingPrice),
                "popular" => books.OrderByDescending(b => b.Popularity),
                "genre" => books.OrderBy(b => b.Genre),
                "year" => books.OrderByDescending(b => b.YearOfPublishing),
                _ => books
            };

            return View("Index", books.ToList()); // Return filtered and sorted books to the Index view
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
