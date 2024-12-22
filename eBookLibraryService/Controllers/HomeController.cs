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

        // Index action - Fetching books with optional filtering, sorting, and searching
        public async Task<IActionResult> Index(
            string query = null,
            string author = null,
            string genre = null,
            string method = null,
            float? minPrice = null,
            float? maxPrice = null,
            bool? isOnSale = null, // New parameter for filtering discounted books
            int? year = null, // New parameter for filtering by publication year
            string publisher = null, // New parameter for filtering by publisher
            string sortOrder = null)
        {
            var books = _context.Books.AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(query))
            {
                books = books.Where(b => EF.Functions.Like(b.Title, $"%{query}%") || EF.Functions.Like(b.Author, $"%{query}%"));
            }

            // Apply filtering
            if (!string.IsNullOrEmpty(author))
            {
                books = books.Where(b => EF.Functions.Like(b.Author, $"%{author}%"));
            }

            if (!string.IsNullOrEmpty(genre))
            {
                books = books.Where(b => b.Genre == genre);
            }

            if (!string.IsNullOrEmpty(method))
            {
                if (method.Equals("buy", StringComparison.OrdinalIgnoreCase))
                {
                    books = books.Where(b => b.BuyingPrice > 0);
                }
                else if (method.Equals("borrow", StringComparison.OrdinalIgnoreCase))
                {
                    books = books.Where(b => b.BorrowPrice > 0);
                }
            }

            if (minPrice.HasValue)
            {
                books = books.Where(b => b.BuyingPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.BuyingPrice <= maxPrice.Value);
            }

            if (isOnSale.HasValue && isOnSale.Value)
            {
                books = books.Where(b => b.DiscountPrice.HasValue && b.DiscountPrice < b.BuyingPrice);
            }

            if (year.HasValue)
            {
                books = books.Where(b => b.PublicationYears.Contains(year.Value));
            }

            if (!string.IsNullOrEmpty(publisher))
            {
                books = books.Where(b => b.Publishers.Contains(publisher));
            }

            // Apply sorting
            books = sortOrder switch
            {
                "price_asc" => books.OrderBy(b => b.BuyingPrice),
                "price_desc" => books.OrderByDescending(b => b.BuyingPrice),
                "popular" => books.OrderByDescending(b => b.Popularity),
                "genre" => books.OrderBy(b => b.Genre),
                "year" => books.OrderByDescending(b => b.YearOfPublishing),
                _ => books
            };

            return View(await books.ToListAsync());
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
