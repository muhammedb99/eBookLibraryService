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

        // Executed before every action
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                // Retrieve or initialize the cart from session
                var cart = HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
                ViewBag.CartItemCount = cart.Items.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing the cart in session.");
                ViewBag.CartItemCount = 0;
            }

            base.OnActionExecuting(context);
        }

        // Main Index action
        public async Task<IActionResult> Index(
    string query = null,
    string author = null,
    string genre = null,
    string method = null,
    float? minPrice = null,
    float? maxPrice = null,
    bool? isOnSale = null,
    int? year = null,
    string publisher = null,
    string sortOrder = null)
        {
            try
            {
                var booksQuery = _context.Books.AsQueryable();

                // Apply filters
                booksQuery = ApplyFilters(booksQuery, query, author, genre, method, minPrice, maxPrice, isOnSale, year, publisher);

                // Apply sorting
                booksQuery = ApplySorting(booksQuery, sortOrder);

                // Fetch all books and group by genre
                var allBooks = await booksQuery.ToListAsync();
                var genres = allBooks
                    .GroupBy(b => b.Genre)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.ToList());

                // Fetch distinct genres for dropdown
                var genreList = await _context.Books
                    .Where(b => !string.IsNullOrEmpty(b.Genre))
                    .Select(b => b.Genre)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToListAsync();

                // Assign data to ViewBag
                ViewBag.Genres = genreList;
                ViewBag.GenreBooks = genres;

                // Log warning if no books are found
                if (!allBooks.Any())
                {
                    _logger.LogWarning("No books found for the given filters.");
                }

                return View(allBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books for the Index page.");
                return View(new List<Book>()); // Return an empty list if an error occurs
            }
        }

        public async Task<IActionResult> Filter(
            string author = null,
            string genre = null,
            string method = null,
            float? minPrice = null,
            float? maxPrice = null,
            bool? isOnSale = null,
            int? year = null,
            string publisher = null)
        {

            var books = _context.Books.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(author))
                books = books.Where(b => EF.Functions.Like(b.Author, $"%{author}%"));
            if (!string.IsNullOrWhiteSpace(genre))
                books = books.Where(b => b.Genre == genre);
            if (!string.IsNullOrWhiteSpace(method))
            {
                if (method.Equals("buy", StringComparison.OrdinalIgnoreCase))
                    books = books.Where(b => b.BuyingPrice > 0);
                else if (method.Equals("borrow", StringComparison.OrdinalIgnoreCase))
                    books = books.Where(b => b.BorrowPrice > 0);
            }
            if (minPrice.HasValue)
                books = books.Where(b => b.BuyingPrice >= minPrice.Value);
            if (maxPrice.HasValue)
                books = books.Where(b => b.BuyingPrice <= maxPrice.Value);
            if (isOnSale.HasValue && isOnSale.Value)
                books = books.Where(b => b.DiscountPrice.HasValue && b.DiscountPrice < b.BuyingPrice);
            if (year.HasValue)
                books = books.Where(b => b.YearOfPublishing == year.Value);
            if (!string.IsNullOrWhiteSpace(publisher))
                books = books.Where(b => b.Publisher.Contains(publisher));
            var filteredBooks = await books.ToListAsync();

            return PartialView("_BooksPartial", filteredBooks); // Render partial view
        }

        // Apply filtering logic
        private IQueryable<Book> ApplyFilters(
            IQueryable<Book> books,
            string query,
            string author,
            string genre,
            string method,
            float? minPrice,
            float? maxPrice,
            bool? isOnSale,
            int? year,
            string publisher)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                books = books.Where(b => EF.Functions.Like(b.Title, $"%{query}%") || EF.Functions.Like(b.Author, $"%{query}%"));
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                books = books.Where(b => EF.Functions.Like(b.Author, $"%{author}%"));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                books = books.Where(b => b.Genre == genre);
            }

            if (!string.IsNullOrWhiteSpace(method))
            {
                books = method.ToLower() switch
                {
                    "buy" => books.Where(b => b.BuyingPrice > 0),
                    "borrow" => books.Where(b => b.BorrowPrice > 0),
                    _ => books
                };
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
                books = books.Where(b => b.YearOfPublishing == year.Value);
            }

            if (!string.IsNullOrWhiteSpace(publisher))
            {
                books = books.Where(b => b.Publisher.Contains(publisher));
            }

            return books;
        }

        // Apply sorting logic
        private IQueryable<Book> ApplySorting(IQueryable<Book> books, string sortOrder)
        {
            return sortOrder switch
            {
                "price_asc" => books.OrderBy(b => b.BuyingPrice),
                "price_desc" => books.OrderByDescending(b => b.BuyingPrice),
                "popular" => books.OrderByDescending(b => b.Popularity),
                "genre" => books.OrderBy(b => b.Genre),
                "year" => books.OrderByDescending(b => b.YearOfPublishing),
                _ => books
            };
        }

        // Privacy action
        public IActionResult Privacy()
        {
            return View();
        }

        // Error action
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
