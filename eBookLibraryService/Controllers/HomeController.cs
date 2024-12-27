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

                // Ensure `ViewBag.GenreBooks` is always initialized
                ViewBag.GenreBooks = genres.Any() ? genres : new Dictionary<string, List<Book>>();

                // Fetch distinct genres for dropdown
                var genreList = await _context.Books
                    .Where(b => !string.IsNullOrEmpty(b.Genre))
                    .Select(b => b.Genre.Trim()) // Ensure no leading/trailing spaces
                    .Distinct()
                    .OrderBy(g => g)
                    .ToListAsync();

                ViewBag.Genres = genreList;

                return View(allBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books for the Index page.");
                ViewBag.GenreBooks = new Dictionary<string, List<Book>>(); // Initialize empty dictionary in case of error
                return View(new List<Book>()); // Return an empty list if an error occurs
            }
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
                books = books.Where(b => (b.DiscountPrice ?? b.BuyingPrice) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => (b.DiscountPrice ?? b.BuyingPrice) <= maxPrice.Value);
            }

            if (isOnSale.HasValue && isOnSale.Value)
            {
                // Include only books with an active discount
                books = books.Where(b =>
                    b.DiscountPrice.HasValue &&
                    b.DiscountPrice < b.BuyingPrice &&
                    b.DiscountUntil.HasValue &&
                    b.DiscountUntil.Value >= DateTime.Now
                );
            }

            if (year.HasValue)
            {
                books = books.Where(b => b.YearOfPublishing == year.Value);
            }

            if (!string.IsNullOrWhiteSpace(publisher))
            {
                books = books.Where(b => EF.Functions.Like(b.Publisher, $"%{publisher}%"));
            }

            return books;
        }



        // Apply sorting logic
        private IQueryable<Book> ApplySorting(IQueryable<Book> books, string sortOrder)
        {
            return sortOrder switch
            {
                "price_asc" => books.OrderBy(b => b.DiscountPrice ?? b.BuyingPrice),
                "price_desc" => books.OrderByDescending(b => b.DiscountPrice ?? b.BuyingPrice),
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
